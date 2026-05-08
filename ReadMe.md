# Crafty Web app
This is an example of a simple e-commerce store that resembles Etsy.
This is meant to be a portfolio piece for demonstrating my knowledge in C#, ASP.NET, and Angular so it is open for issues but closed for pull requests. This repository includes two projects:
- A C#/ASP.NET/Entity Framework backend.
- An Angular frontend.

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

[![Cloud Design](https://mermaid.ink/img/pako:eNqlUsFum0AQ_ZXVnopkExsbHFCVg83RjRpzsFRxmYUxoAKLliVpa_nfu4vBxa6bxOmemJk3780Mb08jHiP16C7nL1EKQpL1JixJ916QrfIMS_lpB94OxjlUkldki8wg4_EDyTnES8ihjFBs6w5UR5DjmB3TMVkrDOlBZBsYf-j1O6doWZVqgOI5i_BEqULdjIx0lc9M3D28WjRN80Kq26jnbrUEQuyzXicGCQxqJBuVJv5yoHJeCp7WN4gUUEsUPrtU2YpMoua6wqFf3zec9G21mK0gSvFSzF-SNv-OeVf-Y9cd5byJdWx8zBSrFOR7bKFxhib_tzk0pOVXNpXduNNzfwRZUkK-adlIfYRcWXdIcGL8gnUNCT412PSnw_IZc17hka8DtL_9a8PugoaRFj0wyS0d163Tzzcc5zTj3w5qdR55b8fXq0qRGLcd2PrfA1tXD0xHNBFZTD0pGhzRAkUBOqR7TRZSmWKBIfXUZwzie0jD8qB6Kii_cV70bYI3SUrVeHmtoqZSe6OfQSKgOGUFlrFajTelpJ5ltxzU29Mf1Js5U3PmLibufGE5tqOLP6k3d03nfmFPFo7lzty5PTuM6K9WdGLqvHqWNZ3c21PXOfwGmKKZ7w?type=png)](https://mermaid.ai/live/edit#pako:eNqlUsFum0AQ_ZXVnopkExsbHFCVg83RjRpzsFRxmYUxoAKLliVpa_nfu4vBxa6bxOmemJk3780Mb08jHiP16C7nL1EKQpL1JixJ916QrfIMS_lpB94OxjlUkldki8wg4_EDyTnES8ihjFBs6w5UR5DjmB3TMVkrDOlBZBsYf-j1O6doWZVqgOI5i_BEqULdjIx0lc9M3D28WjRN80Kq26jnbrUEQuyzXicGCQxqJBuVJv5yoHJeCp7WN4gUUEsUPrtU2YpMoua6wqFf3zec9G21mK0gSvFSzF-SNv-OeVf-Y9cd5byJdWx8zBSrFOR7bKFxhib_tzk0pOVXNpXduNNzfwRZUkK-adlIfYRcWXdIcGL8gnUNCT412PSnw_IZc17hka8DtL_9a8PugoaRFj0wyS0d163Tzzcc5zTj3w5qdR55b8fXq0qRGLcd2PrfA1tXD0xHNBFZTD0pGhzRAkUBOqR7TRZSmWKBIfXUZwzie0jD8qB6Kii_cV70bYI3SUrVeHmtoqZSe6OfQSKgOGUFlrFajTelpJ5ltxzU29Mf1Js5U3PmLibufGE5tqOLP6k3d03nfmFPFo7lzty5PTuM6K9WdGLqvHqWNZ3c21PXOfwGmKKZ7w)

# Getting the code
This project uses a submodule I created called CraftyCommon which you can see among my repositories. Ensure that submodules are downloaded and current when using this repository.

# How to run
The short version is that you can't without making some minor changes to the code. My application uses Cloudinary as a CDN for user-uploaded media files. Naturally I don't want people to use my code to upload inappropriate media to my cloudinary account so I have replaced my credentials with invalid ones. You can still run the application, but you will need to follow these steps:
- Create a cloudinary account (there are free accounts with limits, but this is what I use for development)
- Create an API key
- If not already there, Navigate to your cloudinary dashboard and open your cloudinary keys page.
- Open API/appsettings.json file and enter in the following data:
    - "CloudName": "The Cloud Name At The Top Left Product Environments Dropdown",
    - "ApiKey": "Your API key",
    - "ApiSecret": "Your API secret"

## Running from docker.
This is the recommended way to run this application in a short amount of time, but I have a disclaimer. For reasons I do not yet understand, when the application is run from docker containers, using firefox to view the client causes all requests to the backend to return 500 server error responses. I have tested this application with chrome and edge and those seem to work just fine. To run this application just use the following from a terminal:

```
docker compose up
```

Once it is up and running you should be able to use the app from chrome or edge using the url `http://localhost:4200`
I recommend you login with the credentials:
Username: zamora
Password: password

## Running the code manually
You will need two terminals to run this code unless you want to run them with background processes. I recommend two sessions so you can see the output of each server separately.

### Dependencies
- .NET SDK 9.0
- Sqlite3
- Node.js
- npm
- The angular cli version 18 or newer (npm install -g @angular/cli)
- It is helpful to add dotnet and npm to your PATH in Windows. In Linux, this is usually already handled for you.
- There are numerous third party libraries used by this repository, but they are all installed using npm for the front-end and nuget for the backend using standard npm and nuget repositories.

### Front-end
- cd client
- npm install
- ng serve
Navigate to http://{ip address}:4200

### Back-end
- dotnet clean
- dotnet build
- cd API
- dotnet run
    - If you get CORS errors between client and server, but postman works, then it is likely that the trusted self-signed certificate expired. This is used for testing https communication.
    - Run "dotnet dev-certs https --trust" then restart your browser. That should temporarily trust the self-signed certificate this app uses.

All accounts use "password" as their password. I recommend you use "zamora" for a regular user and "nate" for a user with an administrator role.

The TestDataTemplate in the server application is used with json-generator.com to generate seed data. It was used during early development, but doesn't work now due to numerous changes to the code. It is in my todo list below to update this. Until then, just use the sqlite database included in this repository.

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
- Create the ability to leave craft reviews. This would be a good example of when using a nosql database would be helpful.
- Create a CDN abstraction layer so switching to a different CDN can be as painless as possible. 
- Sqlite doesn't store UTC dates well. I might want to fix that manually. This probably insn't improtant for a portfolio piece. If I were to do this project again I wouldn't use sqlite at all. It was a recommendation from the angular/asp.net course I took that in hindsight was a bad idea. Migrating to another database engine may involve numerous refactors to my code and will definitely require a custom migration script. 
- My client currently passes the raw password when registering for a new account. This is a security flaw. I should consider using a better solution.
- Update seed data json. Now that the app has the ability to create all entities from the front-end, there isn't much need for seed-data.
- Deploy my app using mariadb instead of sqlite. 
    - I chose not to do this because I want people to be able to run my application quickly. If I used mariadb, then I would need to create a custom mariadb container with my database, or create a script that imports a database export on startup if the tables don't exist. Using sqlite I can just bundle my database with the code and you can run.
    - Obfuscate my javascript code during deployment so it is harder to reverse engineer in production.
    - Figure out why running in docker containers causes 500 server errors from all request from Firefox to the backend container on the same machine. Chrome/edge are fine.

# Common Questions
## Why this app
I wanted to create something that was familiar but not as commonly asked in interviews as "Create youtube" so I chose "Create Etsy." Unfortunately, part way through implementing this I realized that "Create Etsy" is no diffent than "Create an e-commerce store", but I was already in too deep so I stuck with it.

## Why not host this yourself for easier viewing
I am not a cybersecurity expert. I don't want to have to spend the time that is necessary to prevent problems like a bot using my frontend to automate creating crafts with inappropriate pictures, videos or 3d models. I want to ensure that when I run my app on my laptop, that my sample data is the only thing