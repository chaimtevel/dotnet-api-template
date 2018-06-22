### Environment

This app runs on dotnet 2.1. It also requires the ability to connect to RabitMq. See [how to setup services](https://git.bitbean.com/coci/ServiceTemplate/blob/master/README.md) to have that up and running quickly.

### Setup

The first time this repo is cloned, run the following script that runs paket restore for the API.

```sh
cd ./Api
../.paket/paket.exe restore
```

### App Settings

We have the appsetting.Development.json file in lastpass as it contains (some) sensitive information.

### Swagger

https://github.com/domaindrivendev/Swashbuckle.AspNetCore

The swagger ui is available at http://localhost/swagger
