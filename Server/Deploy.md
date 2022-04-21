docker build -t put-away-api .\bin\Release\net6.0\publish
docker tag put-away-api registry.heroku.com/put-away-api/web
docker push registry.heroku.com/put-away-api/web
heroku container:release web -a put-away-api