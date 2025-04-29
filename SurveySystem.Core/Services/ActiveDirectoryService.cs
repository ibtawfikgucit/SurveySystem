// Corrected ActiveDirectoryService.cs
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Microsoft.Extensions.Configuration;

namespace SurveySystem.Core.Services
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private readonly IConfiguration _configuration;

        public ActiveDirectoryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool ValidateCredentials(string username, string password)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _configuration["ActiveDirectory:Domain"]))
                {
                    return context.ValidateCredentials(username, password);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public UserAdInfo GetUserInfo(string username)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _configuration["ActiveDirectory:Domain"]))
                {
                    var user = UserPrincipal.FindByIdentity(context, username);

                    if (user == null)
                    {
                        return null;
                    }

                    // Get group memberships
                    var userGroups = GetUserGroups(user);

                    // Check for admin and creator group membership
                    var adminGroup = _configuration["ActiveDirectory:AdminGroup"];
                    var creatorGroup = _configuration["ActiveDirectory:CreatorGroup"];
                    var isAdmin = userGroups.Contains(adminGroup) || userGroups.Contains("Domain Admins");
                    var isCreator = userGroups.Contains(creatorGroup);

                    var userInfo = new UserAdInfo
                    {
                        Username = user.SamAccountName,
                        DisplayName = user.DisplayName,
                        Email = user.EmailAddress,
                        IsAdmin = isAdmin,
                        IsCreator = isCreator,
                        Groups = userGroups
                    };

                    return userInfo;
                }
            }
            catch (Exception ex)
            {
                // Log the error
                return null;
            }
        }

        private List<string> GetUserGroups(UserPrincipal user)
        {
            var groups = new List<string>();

            try
            {
                using (var searcher = new DirectorySearcher())
                {
                    searcher.Filter = $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={user.SamAccountName}))";
                    searcher.PropertiesToLoad.Add("memberOf");

                    var result = searcher.FindOne();
                    if (result != null)
                    {
                        var memberOf = result.Properties["memberOf"];
                        foreach (var groupDn in memberOf)
                        {
                            // Extract CN from the Distinguished Name
                            var groupName = ExtractCN(groupDn.ToString());
                            if (!string.IsNullOrEmpty(groupName))
                            {
                                groups.Add(groupName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
            }

            return groups;
        }

        private string ExtractCN(string distinguishedName)
        {
            try
            {
                // Extract the CN part from a distinguished name
                // Format: CN=GroupName,OU=SecurityGroups,DC=domain,DC=local
                var cnPart = distinguishedName.Split(',')[0];
                return cnPart.Substring(3); // Remove "CN=" prefix
            }
            catch
            {
                return null;
            }
        }

        public List<UserAdInfo> GetUsersInGroup(string groupName)
        {
            var users = new List<UserAdInfo>();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _configuration["ActiveDirectory:Domain"]))
                {
                    var group = GroupPrincipal.FindByIdentity(context, groupName);

                    if (group != null)
                    {
                        var members = group.GetMembers();

                        foreach (var member in members)
                        {
                            if (member is UserPrincipal user)
                            {
                                // Get groups for this user
                                var userGroups = GetUserGroups(user);

                                // Check for admin and creator group membership
                                var adminGroup = _configuration["ActiveDirectory:AdminGroup"];
                                var creatorGroup = _configuration["ActiveDirectory:CreatorGroup"];
                                var isAdmin = userGroups.Contains(adminGroup) || userGroups.Contains("Domain Admins");
                                var isCreator = userGroups.Contains(creatorGroup);

                                users.Add(new UserAdInfo
                                {
                                    Username = user.SamAccountName,
                                    DisplayName = user.DisplayName,
                                    Email = user.EmailAddress,
                                    IsAdmin = isAdmin,
                                    IsCreator = isCreator,
                                    Groups = userGroups
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
            }

            return users;
        }

        // Method to check if a user is a member of a specific AD group
        public bool IsUserInGroup(string username, string groupName)
        {
            try
            {
                var userInfo = GetUserInfo(username);
                return userInfo?.Groups.Contains(groupName) ?? false;
            }
            catch
            {
                return false;
            }
        }
    }

    public interface IActiveDirectoryService
    {
        bool ValidateCredentials(string username, string password);
        UserAdInfo GetUserInfo(string username);
        List<UserAdInfo> GetUsersInGroup(string groupName);
        bool IsUserInGroup(string username, string groupName);
    }

    public class UserAdInfo
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsCreator { get; set; }
        public List<string> Groups { get; set; } = new List<string>();
    }
}