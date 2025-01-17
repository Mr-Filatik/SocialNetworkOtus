# SocialNetworkOtus

Social network project for the highload architect course from the Otus school.
 
## Launching the application

### 1. Launching the docker container

Clone this repository.

Go to the project folder in the console.

Run docker container.

```console
docker-compose up -d --force-recreate --build backend
```

In postgres volumes in file pg_hba.conf add `host replication all 172.21.0.0/16 trust` for allow connection, and restart containers.

Wait 5-7 seconds for the application to start and initialize.

### 2. Testing the work

Open Postman and export collections from the `Postman` folder.

Run queries one by one.

## Description of requests

1. `Get profiles (no-auth)` - attempt to get profiles without authorization;
2. `Add new user` - register a new user;
3. `Get my profile (no-auth)` - attempt to get your profile without authorization;
4. `Login` - authorization and receiving a token;
5. `Get my profile (auth)` - successful receipt of your profile;
6. `Get profiles (auth)` - successful receipt of profiles.

## Development stack

* Language: `C#`
* Platform: `.NET Core`
* Database: `PostgreSQL`

## Postman documentation

The Postman collection for testing APIs is located in the `Postman` folder. Differences in versions of json at the time of unloading.

## About developer

This project is being developed by Vladislav Filatov.