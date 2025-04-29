"# SurveySystem" 


git commands

git init
git add README.md
echo "# SurveySystem" >> README.md
git add README.md
git commit -m "first commit"
git branch -M main
git remote add origin https://github.com/ibtawfikgucit/SurveySystem.git
git push -u origin main
dotnet new gitignore
git add .
git commit -m "initial files"
git push -u origin main



git rm -r --cached SurveySystem.Web\wwwroot\lib
git commit -m "Remove wwwroot lib folder"
git push -u origin main



dotnet ef migrations add "allow null in Question Validation Regex" --startup-project SurveySystem.API --project SurveySystem.Infrastructure
dotnet ef database update --startup-project SurveySystem.API --project SurveySystem.Infrastructure
