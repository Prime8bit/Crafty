# Why don't you use kubernetes
I researched kubernetes and how it could benefit my project. What I determined was that while it is a powerful tool and is very helpful in scaling up an application, the added complexity isn't worth it until I have numerous servers that need to be orchestrated. I am including my kubernetes/helm scripts here as a reference, but I will not be using them any time soon. 

## Limitations of the code for kubernetes
This should go without saying, but I will continue to make changes to the code and maintaining a set of kubernetes scripts that I am not actually using is time consuming. I will keep these here as a reference later, should I decide to use kubernetes, but these scripts are likely out of date with my docker efforts. 
I have implemented rate limiting soon and the solution I will use doesn't transfer well to a clustered environment. The problem is that until you have a clustered environment, using the clustered solution is unnecessary. The rate limits will still work, just not as intended. For example, if you have 1 request per user per minute as a rate limit. This will be enforced on a *per pod basis*. If you have three pods each will permit one request per user per minute, but a user can get around this a bit if the load balancer shifts subsequent requests to other pods effectively allowing for a worst case of n requests per minute where n is the number of pods. This can be fixed by using a temporary data store like redis to syncronize counts with the rate limiting libraries in ASP.NET, but this solution is unnecessary when you only have a single instance of an application. 

# How to build with k8S
## Development environment notes
It should be noted that I only ever tested these scripts on a windows docker installation using k3d. k3d is a small kubernetes implementation that works well on devices with limited ram and processing power. These scripts also require helm, a templating engine for yaml scripts. There are numerous guides on how to install k3d, helm and docker so I will not post them here. Once you have them installed you may proceed with the instructions below. I use helm to create templated kuberenetes yaml files. Helm and some kubernetes implementation, like k3d, need to be installed to continue.

## Build instructions
First you must build the docker compose file. This creates local versions of the docker images that k8s will use. 

```
cd ../api
docker compose --profile production build
cd ../k8s
```

Start a k3d cluster with ports 80 and 443 forwarded. It is wise to verify the cluster is up and running

```
k3d cluster create craftyCluster -p "80:80@loadbalancer" -p "443:443@loadbalancer"
kubectl get pods
```

Once the cluster is working import the devopment images for the frontend and backend into the cluster.

```
k3d image import crafty-frontend:latest crafty-backend:latest -c craftyCluster
```

Now install the kubernetes cluster using helm. Verify all the pods have the running status. This will take a few minutes.

```
helm install crafty . -f values-dev.yaml
kubectl get pods
```

Once the pods are up and running you should be able to access the application from http://localhost.