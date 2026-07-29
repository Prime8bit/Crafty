# Crafty Web app
This is an example of a simple e-commerce store that resembles Etsy.
This is meant to be a portfolio piece for demonstrating my knowledge in C#, ASP.NET, and Angular so it is open for issues but closed for pull requests. This repository includes two projects:
- A C#/ASP.NET/Entity Framework backend.
- An Angular/Typescript frontend.

# Design
The original design for this application is deliberately simple. Because this is a portfolio piece, ease of running on a single machine was prioritized over typical good scaling practices. This is the design right now:

<!-- I can't use a comment here because mermaid code contains the end comment sequence -->
<details>
<summary>Mermaid source (hidden)</summary>

    flowchart LR
        webClient(fa:fa-laptop Web)

        webClient --> server(fa:fa-server Server)
            server --> db(fa:fa-database Database)
            server --> cdn(fa:fa-database CDN)

</details>

[![Portable Design](https://mermaid.ink/img/pako:eNptkbtuwzAMRX9F4NQATmDJ8ktDl2RsO7RDgcKLbNEPwJYMRW7aGvn3-tUOQbiQlzj3cuAIhVEIAsrWXIpaWkeeXjNNprpgfmwb1O6hlKKU-1b2zvTkHfNdpm8Qst8_kjPaT7QbvQrytrTdis-17Wde5RurpJO5PCM5bcN9vlD61nA8vezAg8o2CoSzA3rQoe3kLGGcUzJwNXaYgZjGtqlql0Gmr5Opl_rDmO7PZ81Q1TClt-dJDf10A0-NrKzs_rcWtUJ7NIN2IELO6JICYoQvEIxGh5T7gR9HNKZJHHrwvW6TwE849SPOw4Cyqwc_y13_EPGQsYjFjCc09WnqAarGGfu8_mR5zfUXuKKA3Q?type=png)](https://mermaid.ai/live/edit#pako:eNptkbtuwzAMRX9F4NQATmDJ8ktDl2RsO7RDgcKLbNEPwJYMRW7aGvn3-tUOQbiQlzj3cuAIhVEIAsrWXIpaWkeeXjNNprpgfmwb1O6hlKKU-1b2zvTkHfNdpm8Qst8_kjPaT7QbvQrytrTdis-17Wde5RurpJO5PCM5bcN9vlD61nA8vezAg8o2CoSzA3rQoe3kLGGcUzJwNXaYgZjGtqlql0Gmr5Opl_rDmO7PZ81Q1TClt-dJDf10A0-NrKzs_rcWtUJ7NIN2IELO6JICYoQvEIxGh5T7gR9HNKZJHHrwvW6TwE849SPOw4Cyqwc_y13_EPGQsYjFjCc09WnqAarGGfu8_mR5zfUXuKKA3Q)


## Design for scale
If I were to change this application to design for scale this is the design I would propose. Since this does process purchases, I would prioritize consistency over availability. If the system were to become partitioned, then the partitioned services would simply shut down, routing all their traffic to the remaining services that are connected to the master system. I considered breaking this apart further so that browsing and searching of crafts used a high availability service while the orders used a separate high consistency service, but I think it is too early for that level of optimization. This seemed like a good place to start.

I would split the web service into multiple microservices. At the very least I would have an authentication service, a chat service, and one service for the rest of the site. Authentication is needed for both chat and the rest of the site, so I would break it out so that if one of these two goes down, the other can still operate. 
I would use Redis as a memcache for the database queries to improve response times and reduce load on the servers. I would use continue to use SQL for most web services because there are a lot of relationships between various entity types that cannot be migrated well to NoSQL and I want the ACID safety that SQL provides for purchase information. I would use NoSQL for chat because it doesn't have complex relationships, so ACID can still be maintained with few tweaks to the schema, if any. Because there are likely to be many chat messages in a small amount of time, horizontal scaling is a higher priority than with other services. 
I would use Kafka as a message queue between the SignalR service and the NoSQL databases for a few reasons. First, I don't lose messages if the database goes down for maintenance. Second is it creates redundancy. If the message queue goes down I can still write direct to database. If the database goes down I can still write messages to the queue, but they won't be delivered until the database comes back up. 

<!-- I can't use a comment here because mermaid code contains the end comment sequence -->
<details>
<summary>Mermaid source (hidden)</summary>

    flowchart LR
        webClient(fa:fa-laptop Web) --> loadBalancerWs(fa:fa-scale-balanced Load Balancer WS)
            loadBalancerWs --> webServices(fa:fa-server Web Service<br/>fa:fa-server Web Service<br/>...)
                webServices --> readDbs(fa:fa-database Read DB<br/>fa:fa-database Read SQL<br/>...)
                webServices --> masterDb(fa:fa-database Write SQL)
                    masterDb --> readDbs
                webServices --> dbCache(fa:fa-database DB Cache)
                webServices --> CDN(fa:fa-cloud CDN)
        webClient(fa:fa-laptop Web) --> loadBalancerChat(fa:fa-scale-balanced Load Balancer Chat)    
            loadBalancerChat --> chatService1(fa:fa-server SignalR Chat service)
                chatService1 --> chatMessageQueue(fa:fa-envelope Chat Message<br/>Pub/Sub Queue<br/>fa:fa-envelope Chat Message<br/>Pub/Sub Queue<br/>...)
                    chatMessageQueue --> chatDb(fa:fa-database Chat NoSQL<br/>fa:fa-database Chat NoSQL<br/>... )
            loadBalancerChat --> chatService2(fa:fa-server SignalR Chat service)
                chatService2 --> chatMessageQueue

</details>

[![Cloud Design](https://mermaid.ink/img/pako:eNqlVMGO2jAQ_RXLpyJBFgKETVTtAXJkV104IK24jJMhieTEkePstkX8e22TsIHS7dLOKeN589548uQ9jUSMNKA7Lt6iFKQiy9W2IE28IVvwDAv1ZQfBDgYcSiVKskHWI4PBA-EC4jlwKCKUm6oBVRFwHLDjcUyWGkNaENmse-_0Js4pLKtWXaN8zSI8UerUNCMjTeUrk3cPHxYdx7mQam7UclstiRCHrNWJQQGDCslKH5Nw3lE5L62flzeI5FAplCG7VNnITKHhusJhou3rTvp3tZgtIErxUiycE3v-iXkX4VPTHXFRxybv_ZspFimoz9jC4HqG_M_mMBDLr22qmnFH5_5YZ0kBfGXZSHWEXLlul-DE-IhVBQk-11i3q8PiFbko8cjXAOxv_1azu3XNiEV3THJLx3XrtPN1xznN-LuDrM6TaO34cVUrkt5tC3b_d8Hu1QXTPk1kFtNAyRr7NEeZg0np3pBtqUoxxy0N9CfPklRt6bY46KYSihch8rZPijpJqZ6PVzqrS31xDDNIJLxDsIj11URdKBqMPEtBgz39ToOxN3LG_mzoT2auN_XcaZ_-oMHEd7z72XQ481x_7E-m40Of_rSaQ8ec6xi505F_7w3HfYpxpoR8PL6h9ik9_AL9VqCw?type=png)](https://mermaid.ai/live/edit#pako:eNqlVMGO2jAQ_RXLpyJBFgKETVTtAXJkV104IK24jJMhieTEkePstkX8e22TsIHS7dLOKeN589548uQ9jUSMNKA7Lt6iFKQiy9W2IE28IVvwDAv1ZQfBDgYcSiVKskHWI4PBA-EC4jlwKCKUm6oBVRFwHLDjcUyWGkNaENmse-_0Js4pLKtWXaN8zSI8UerUNCMjTeUrk3cPHxYdx7mQam7UclstiRCHrNWJQQGDCslKH5Nw3lE5L62flzeI5FAplCG7VNnITKHhusJhou3rTvp3tZgtIErxUiycE3v-iXkX4VPTHXFRxybv_ZspFimoz9jC4HqG_M_mMBDLr22qmnFH5_5YZ0kBfGXZSHWEXLlul-DE-IhVBQk-11i3q8PiFbko8cjXAOxv_1azu3XNiEV3THJLx3XrtPN1xznN-LuDrM6TaO34cVUrkt5tC3b_d8Hu1QXTPk1kFtNAyRr7NEeZg0np3pBtqUoxxy0N9CfPklRt6bY46KYSihch8rZPijpJqZ6PVzqrS31xDDNIJLxDsIj11URdKBqMPEtBgz39ToOxN3LG_mzoT2auN_XcaZ_-oMHEd7z72XQ481x_7E-m40Of_rSaQ8ec6xi505F_7w3HfYpxpoR8PL6h9ik9_AL9VqCw)

# Getting the code
This project uses a submodule I created called CraftyCommon which you can see among my repositories. Ensure that submodules are downloaded and current when using this repository.

# How to run
The short version is that you can't without making some minor changes to the code. My application uses Cloudinary as a CDN for user-uploaded media files. Naturally I don't want people to use my code to upload inappropriate media to my cloudinary account so I have replaced my credentials with invalid ones. You can still run the application, but you will need to follow the steps below

## Setting up the environment
Regardles of whether you run from docker or locally, you will need to set up the environment.
- Copy /.env_sample to /.env
- Create a cloudinary account (there are free accounts with limits, but this is what I use for development)
- Create an API key
- If not already there, Navigate to your cloudinary dashboard and open your cloudinary keys page.
- Update your .env file with all your cloudinary and database values
    - If you are wondering about the JWT_TOKEN_KEY, it is only used for hashing passwords and can be any string you wish, but you must restart your asp.net application for changes to take effect.

## Running from docker
This is the easiest way to simply run this application. You will need to install docker and follow the instructions for setting up the environment for this to work.
All you should need to do is run:
    docker compose --profile proudction build --no-cache
    docker compose --profile production up
A sample database is already provided so you can login as "zamora" with password "password" for a normal user or "nate" with password "password" for an admin.

## Running the code manually
You will need three terminals to run this code unless you want to run them with background processes. I recommend three sessions so you can see the output of each server separately.

### Dev environment setup
A few additional steps need to be taken to run the code manually for the ASP.NET application
- Copy API/appsettings.json to API/appsettings.Development.json. It has already been added to .gitignore so it should be safe to check in without exposing your keys. It is always good to double check.
- Open API/appsettings.Development.json file and enter in the following data:
    - "CloudinarySettings.CloudName": "The Cloud Name At The Top Left Product Environments Dropdown",
    - "CloudinarySettings.ApiKey": "Your cloudinary API key",
    - "CloudinarySettings.ApiSecret": "Your cloudinary API secret"
    - The JWT_TOKEN_KEY can be anything you want
    - "POSTGRES_HOST": "localhost" You specifically want to override this for local development so you can run the backend from your IDE and have it communicate with your database.
    - Everything else can derive from appsettings.json. If you want to change the port, username, or password you must do so in .env so the database and ASP.NET application match.

### Dependencies
- .NET SDK 9.0
- Docker
- Node.js
- npm
- The angular cli version 18 or newer (npm install -g @angular/cli)
- It is helpful to add dotnet and npm to your PATH in Windows. In Linux, this is usually already handled for you.
- There are numerous third party libraries used by this repository, but they are all installed using npm for the front-end and nuget for the backend using standard npm and nuget repositories.

### Front-end
- cd client
- npm install
- ng serve
Navigate to http://localhost:4200

### Back-end
- In Terminal 1, run:
    - docker compose build --no-cache 
        - This forces a rebuild of all containers with the latest code from the repository
    - docker compose up
        - This will boot up a postgresql server and insert some test data.
- cd API 
- dotnet run
    - If you get CORS errors between client and server, but postman works, then it is likely that the trusted self-signed certificate expired. This is used for testing https communication.
    - Run "dotnet dev-certs https --trust" then restart your browser. That should temporarily trust the self-signed certificate this app uses.

All accounts use "password" as their password. I recommend you use "zamora" for a regular user and "nate" for a user with an administrator role.

## Ideas for 2.0+:
- Currently, the update craft page has a huge flaw - it is possible to delete images then "cancel" the update. 
    - This should be prevented as it will cause the images to fail to load and because they have been deleted already, trying to delete them again will fail.
    - I also need to do something when a user uploads new media, but then cancels the creation/updating of crafts. This will leak storage space on my CDN.
- I need to handle the use case where a seller updates crafts between the time a user adds items to their cart and the time they checkout.
- I would like to use MVVM for the front-end. I don't know if this is wise and I am certain that google recommends against OOP in angular for greater efficiency. This is something I will have to research.
- Add logging and reporting.
- Convert the craftSort option into an enum in wishlistscomponent and craftlistscomponent.
- Implement separate billing and shipping addresses.
- Create a curated list of categories for crafts and allow users to assign categories to their crafts. This has the potential to be abused if a person puts all categories on their products so I would need to somehow prevent that.
- Create the ability to leave craft reviews.
- Create a CDN abstraction layer so switching to a different CDN can be as painless as possible. 
- My client currently passes the raw password when registering for a new account. This is a major security flaw and should be hashed before being sent, even when using https.
- Delete seed data json. Now that the app has the ability to create all entities from the front-end and I have included a sample database, there isn't much need for seed-data.

# Common Questions
## Why this app
I wanted to create something that was familiar but not as commonly asked in interviews as "Create youtube" so I chose "Create Etsy." Unfortunately, part way through implementing this I realized that "Create Etsy" is no diffent than "Create an e-commerce store", but I was already in too deep so I stuck with it.

## Why not host this yourself for easier viewing
I am not a cybersecurity expert. I don't want to have to spend the time that is necessary to prevent problems like a bot using my frontend to automate creating crafts with inappropriate pictures, videos or 3d models. I want to ensure that when I run my app on my laptop, that my sample data is the only thing