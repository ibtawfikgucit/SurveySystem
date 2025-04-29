using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SurveySystem.Core.Interfaces;
using SurveySystem.Core.Models;
using System.Linq.Expressions;
using System.Text.Json;

namespace SurveySystem.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        //private readonly IAuditService _auditService;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ICurrentUserService currentUserService//,
            //IAuditService auditService
            ) : base(options)
        {
            _currentUserService = currentUserService;
            //_auditService = auditService;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<SurveyResponse> SurveyResponses { get; set; }
        public DbSet<QuestionResponse> QuestionResponses { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Survey configuration
            modelBuilder.Entity<Survey>()
                .HasOne(s => s.CreatedByUser)
                .WithMany(u => u.CreatedSurveys)
                .HasForeignKey(s => s.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Question configuration
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Survey)
                .WithMany(s => s.Questions)
                .HasForeignKey(q => q.SurveyId)
                .OnDelete(DeleteBehavior.Restrict); ;

            // QuestionOption configuration
            modelBuilder.Entity<QuestionOption>()
                .HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); ;

            // SurveyResponse configuration
            modelBuilder.Entity<SurveyResponse>()
                .HasOne(r => r.Survey)
                .WithMany(s => s.Responses)
                .HasForeignKey(r => r.SurveyId)
                .OnDelete(DeleteBehavior.Restrict); ;

            modelBuilder.Entity<SurveyResponse>()
                .HasOne(r => r.Respondent)
                .WithMany(u => u.SurveyResponses)
                .HasForeignKey(r => r.RespondentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); // Allow anonymous responses

            // QuestionResponse configuration
            modelBuilder.Entity<QuestionResponse>()
                .HasOne(qr => qr.SurveyResponse)
                .WithMany(sr => sr.QuestionResponses)
                .HasForeignKey(qr => qr.SurveyResponseId)
                .OnDelete(DeleteBehavior.Restrict); ;

            modelBuilder.Entity<QuestionResponse>()
                .HasOne(qr => qr.Question)
                .WithMany(q => q.Responses)
                .HasForeignKey(qr => qr.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); ;

            modelBuilder.Entity<QuestionResponse>()
                .HasOne(qr => qr.SelectedOption)
                .WithMany(o => o.Responses)
                .HasForeignKey(qr => qr.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); // Not all questions have options

            // Apply global query filter for soft delete
            ApplySoftDeleteQueryFilter(modelBuilder);
        }

        private void ApplySoftDeleteQueryFilter(ModelBuilder modelBuilder)
        {
            // Apply filter to all entities that inherit from BaseEntity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, "IsDeleted");
                    var condition = Expression.Equal(property, Expression.Constant(false));
                    var lambda = Expression.Lambda(condition, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Audit and timestamp entries
            await ProcessAuditableEntitiesAsync();

            // Track changes for audit log
            await TrackChangesAsync();

            return await base.SaveChangesAsync(cancellationToken);
        }

        private async Task ProcessAuditableEntitiesAsync()
        {
            var userId = _currentUserService.GetCurrentUserId();
            var username = _currentUserService.GetCurrentUsername();
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = username;
                        entry.Entity.IsDeleted = false;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedAt = now;
                        entry.Entity.ModifiedBy = username;
                        break;
                    case EntityState.Deleted:
                        // Handle soft delete
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.ModifiedAt = now;
                        entry.Entity.ModifiedBy = username;
                        break;
                }
            }
        }

        private async Task TrackChangesAsync()
        {
            var auditEntries = new List<AuditEntry>();
            var userId = _currentUserService.GetCurrentUserId();
            var username = _currentUserService.GetCurrentUsername();
            var ipAddress = _currentUserService.GetCurrentIpAddress();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry
                {
                    UserId = userId,
                    Username = username,
                    EntityName = entry.Entity.GetType().Name,
                    Action = entry.State.ToString(),
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress
                };

                // Get primary key value
                if (entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue is Guid id)
                {
                    auditEntry.EntityId = id.ToString();
                }

                // Get changed properties
                foreach (var property in entry.Properties)
                {
                    // Skip certain properties
                    if (property.Metadata.IsPrimaryKey() ||
                        property.Metadata.Name == nameof(BaseEntity.CreatedAt) ||
                        property.Metadata.Name == nameof(BaseEntity.CreatedBy) ||
                        property.Metadata.Name == nameof(BaseEntity.ModifiedAt) ||
                        property.Metadata.Name == nameof(BaseEntity.ModifiedBy))
                        continue;

                    // Handle different states
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.OldValues[property.Metadata.Name] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[property.Metadata.Name] = property.OriginalValue;
                                auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue;
                            }
                            break;
                    }
                }

                auditEntries.Add(auditEntry);
            }

            // Save audit logs
            foreach (var auditEntry in auditEntries)
            {
                AuditLogs.Add(new AuditLog
                {
                    UserId = auditEntry.UserId,
                    Username = auditEntry.Username,
                    EntityName = auditEntry.EntityName,
                    EntityId = auditEntry.EntityId,
                    Action = auditEntry.Action,
                    Timestamp = auditEntry.Timestamp,
                    OldValues = JsonSerializer.Serialize(auditEntry.OldValues),
                    NewValues = JsonSerializer.Serialize(auditEntry.NewValues),
                    IpAddress = auditEntry.IpAddress
                });
            }
        }
    }

    // Helper class for tracking audit entries
    public class AuditEntry
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> OldValues { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; set; } = new Dictionary<string, object>();
        public string IpAddress { get; set; }
    }
}