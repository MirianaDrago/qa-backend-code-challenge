# qa-backend-code-challenge

Code challenge for QA Backend Engineer candidates.

### Build Docker image

Run this command from the directory where there is the solution file.

```
docker build -f src/Betsson.OnlineWallets.Web/Dockerfile .
```

### Run Docker container

```
docker run -p <port>:8080 <image id>
```

### Open Swagger

```
http://localhost:<port>/swagger/index.html
```

### API tests configuration

- Change ApiBaseUrl in settings.json file under ApiTests project in order to be able to run API tests according to url set

### Run tests through CLI

```
dotnet test
```

