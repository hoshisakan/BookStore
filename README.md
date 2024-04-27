# HoshiBook

## About
Bhrugen's Udemy Course : Complete guide to ASP.NET Core MVC (6.0) and Entity Framework web development, including how to building a shopping cart with payment integration and more.

## Use Docker to build the following services
* Nginx 1.23.3
* PostgreSQL 15.2
* pgAdmin 7.1
* Dotnet Core 6.0 through Ubuntu 22.10
* Redis 7.0.9

## Browse the web application
### Customer
※ Product list aren't for sale and commercial, just for demo
* Open the browser and enter the URL: https://localhost
* You will see the following user login page
![alt text](https://imgur.com/5VZ3vjG.png)

* Click the "Register" button to register a new user
![alt text](https://imgur.com/TQVEsuP.png)

* Enter the user name and password to login
![alt text](https://imgur.com/ssq6YUm.png)

* You will see the following page (Product list aren't for sale and commercial, just for demo)
![alt text](https://imgur.com/Ie552i8.png)

* Click details to view the product details
![alt text](https://imgur.com/MKrl42p.png)

* You will see the cart quantity on the top left corner of the page after add the product to the shopping cart
![alt text](https://imgur.com/UbMT8U8.png)

* Click the shopping cart icon to view the shopping cart
![alt text](https://imgur.com/KNBwPWZ.png)
![alt text](https://imgur.com/bkW4xpm.png)

* Place an order by clicking the "Place Order" button
![alt text](https://imgur.com/i47ShSD.png)

* You will see the order status is "Approved" after the payment is successful
![alt text](https://imgur.com/nQPkSnf.png)

* Click the Edit icon button right side of the page to view the order details
![alt text](https://imgur.com/yYG7ekC.png)

### Admin
* Enter the user name and password to login
![alt text](https://imgur.com/Al263Ow.png)

* You will see the following page
![alt text](https://imgur.com/NHfNkKC.png)

* Click the "Content Management" -> "Category" button to view the dashboard
![alt text](https://imgur.com/2f8ln30.png)

* Click the "Cover Type Management" -> "Category" button to view the dashboard
![alt text](https://imgur.com/9CxjC9o.png)

* Click the "Product Management" -> "Category" button to view the dashboard
![alt text](https://imgur.com/dgYU2my.png)

* Click the "Company Management" -> "Category" button to view the dashboard
![alt text](https://imgur.com/RwYKrZ0.png)

* Click the "Manage User" button to view the user management page
![alt text](https://imgur.com/Ik8rTM1.png)

* Click "Locked" link display the locked user list
![alt text](https://imgur.com/4Yw9lbi.png)

* Click "Unlocked" link display the unlocked user list
![alt text](https://imgur.com/JtkZ6ZK.png)

* Click the "Manage Order" button to view the order management page
![alt text](https://imgur.com/GbGEDr2.png)

* Click the "SHIP ORDER" button to ship the order
![alt text](https://imgur.com/psNRt3M.png)

* Go back to the order management page, you will see the order status is "Shipped"
![alt text](https://imgur.com/lswwUhk.png)

## Build the Docker services
### Reverse Proxy Server
#### Create a Dockerfile build Nginx version number 1.21.3
* Install the Nginx, and install packages
```
FROM nginx:1.23.3

# Add arguments to set timezone
ARG NGINX_TIME_ZONE
ARG NGINX_LANG_NAME
ARG NGINX_LANG_INPUTFILE
ARG NGINX_LANG_CHARMAP
ARG DEBIAN_FRONTEND

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Install base packages
RUN apt update
RUN apt install -y locales wget gnupg2 apt-transport-https \
    ca-certificates curl software-properties-common \
    libnss3-tools iputils-ping telnet net-tools

# Set timezone to Asia/Taipei
RUN ln -sf /usr/share/zoneinfo/${NGINX_TIME_ZONE} /etc/localtime
# Reset tzdata software package let user set timezone take effect
RUN dpkg-reconfigure -f noninteractive tzdata

# Clear package lists
RUN rm -rf /var/lib/apt/lists/*
```

### Based on the Ubuntu 20.04 image, install the dotnet core 6.0 SDK and runtime
#### Create a Dockerfile build Ubuntu version number 20.04
* Install the dotnet core 6.0 SDK and runtime
```
FROM ubuntu:22.10

ARG DOTNET_IMAGE_VERSION
ARG DOTNET_POSTGRESQL_CLIENT_HOME
ARG DOTNET_POSTGRESQL_CLIENT_VERSION
ARG POSTGRES_DATA_BACKUP_PATH
ARG DOTNET_PACKAGES_PATH
```

* Add the Microsoft package signing key to your list of trusted keys and add the package repository
```
RUN wget https://packages.microsoft.com/config/ubuntu/22.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb
```

* Install the dotnet core 6.0 SDK and runtime
```
RUN apt install -y dotnet-sdk-${DOTNET_IMAGE_VERSION}.0
RUN dotnet tool install --global dotnet-ef --version ${DOTNET_IMAGE_VERSION}.*
```

* Install the PostgreSQL client 15.2 for scheduling backup database
```
RUN wget https://ftp.postgresql.org/pub/source/v${DOTNET_POSTGRESQL_CLIENT_VERSION}/postgresql-${DOTNET_POSTGRESQL_CLIENT_VERSION}.tar.gz

RUN tar -zxvf postgresql-${DOTNET_POSTGRESQL_CLIENT_VERSION}.tar.gz -C ${DOTNET_PACKAGES_PATH}

WORKDIR ${DOTNET_PACKAGES_PATH}/postgresql-${DOTNET_POSTGRESQL_CLIENT_VERSION}

RUN ./configure --prefix=${DOTNET_POSTGRESQL_CLIENT_HOME} && make && make install

RUN rm -rf ${DOTNET_PACKAGES_PATH}/postgresql-${DOTNET_POSTGRESQL_CLIENT_VERSION}.tar.gz && rm -rf ${DOTNET_PACKAGES_PATH}/postgresql-${DOTNET_POSTGRESQL_CLIENT_VERSION}
```

* Run the dll file of dotnet web application in the container
```
WORKDIR /deploy/HoshiBookWeb
ENTRYPOINT [ "dotnet", "HoshiBookWeb.dll" ]
```

### Create docker-compose.yml file to build the container
* docker-compose.yml file content is as follows
```
version: '3.9'
services:
    redis:
        build: ./conf/redis
        image: hoshisakan/bookstore/redis:6.2.5
        container_name: redis-dev
        env_file:
          - ./.env
        environment:
            - REDIS_AOF_ENABLED=${REDIS_AOF_ENABLED}
            - REDIS_PWD=${REDIS_PWD}
        volumes:
            - ./data/redis:/data
            - ./conf/redis/setting/redis.conf:/usr/local/etc/redis/redis.conf
        ports:
            - 6379:6379
        networks:
            bookstore_common_net:
              ipv4_address: ${REDIS_HOST_IP}
        command: redis-server --appendonly yes --requirepass ${REDIS_PWD}
        restart: on-failure:3

    reverse_proxy:
        build:
            context: ./conf/nginx
            dockerfile: Dockerfile
            args:
                - NGINX_TIME_ZONE=${NGINX_TIME_ZONE}
                - NGINX_LANG_NAME=${NGINX_LANG_NAME}
                - NGINX_LANG_INPUTFILE=${NGINX_LANG_INPUTFILE}
                - NGINX_LANG_CHARMAP=${NGINX_LANG_CHARMAP}
                - DEBIAN_FRONTEND=${NGINX_DEBIAN_FRONTEND}
        image: hoshisakan/bookstore/nginx:1.23.3
        container_name: nginx-dev
        env_file:
          - ./.env
        environment:
          - LANG=${NGINX_LANG_NAME}
        volumes:
            - ./web/temp/dist/index.html:/web/dist/index.html
            - ./conf/nginx/nginx.conf:/etc/nginx/nginx.conf
            - ./conf/nginx/conf.d:/etc/nginx/conf.d
            - ./web/deploy/certs:/etc/nginx/ssl
            - ./logs/nginx:/var/log/nginx
        ports:
            - 80:80
            - 443:443
        depends_on:
            - postgres_db
            - redis
            - web
        networks:
          bookstore_common_net:
              ipv4_address: ${NGINX_HOST_IP}
        user: root
        tty: true
        restart: on-failure:3

    web:
      build:
          context: ./conf/dotnet/6.0/ubuntu
          dockerfile: Dockerfile
          args:
              - DOTNET_IMAGE_VERSION=${DOTNET_IMAGE_VERSION}
              - DOTNET_TIME_ZONE=${DOTNET_TIME_ZONE}
              - DOTNET_LANG_NAME=${DOTNET_LANG_NAME}
              - DOTNET_LANG_INPUTFILE=${DOTNET_LANG_INPUTFILE}
              - DOTNET_LANG_CHARMAP=${DOTNET_LANG_CHARMAP}
              - DEBIAN_FRONTEND=${DOTNET_DEBIAN_FRONTEND}
              - DOTNET_POSTGRESQL_CLIENT_HOME=${DOTNET_POSTGRESQL_CLIENT_HOME}
              - DOTNET_POSTGRESQL_CLIENT_VERSION=${DOTNET_POSTGRESQL_CLIENT_VERSION}
              - POSTGRES_DATA_BACKUP_PATH=:${POSTGRES_DATA_BACKUP_PATH}
              - DOTNET_PACKAGES_PATH=${DOTNET_PACKAGES_PATH}
      image: hoshisakan/bookstore/dotnet:6.0
      container_name: web-dev
      env_file:
          - ./.env
      environment:
          - DOTNET_LANG_NAME=${DOTNET_LANG_NAME}
          - PGPASSWORD=${POSTGRES_PASSWORD}
          - DOTNET_POSTGRES_USER=${POSTGRES_USER}
          - DOTNET_POSTGRES_HOST_IP=${POSTGRES_HOST_IP}
          - DOTNET_POSTGRES_PORT=${POSTGRES_PORT}
      volumes:
          - ./web/HoshiBook:/app
          - ./web/deploy/HoshiBookWeb:/deploy/HoshiBookWeb
          - ./web/deploy/certs:/deploy/certs
          - ./web/deploy/staticfiles:/deploy/staticfiles
          - ./data/postgresql/pgdata_backup:${POSTGRES_DATA_BACKUP_PATH}
          - ./logs/dotnet:/var/log/dotnet
      expose:
        - 5002
        - 7232
      depends_on:
        - postgres_db
        - redis
      networks:
          bookstore_common_net:
              ipv4_address: ${DOTNET_HOST_IP}
      tty: true
      restart: on-failure:3

    postgres_db:
        build:
            context: ./conf/postgresql
            dockerfile: Dockerfile
            args:
                - POSTGRES_TIME_ZONE=${POSTGRES_TIME_ZONE}
                - POSTGRES_LANG_NAME=${POSTGRES_LANG_NAME}
                - POSTGRES_LANG_INPUTFILE=${POSTGRES_LANG_INPUTFILE}
                - POSTGRES_LANG_CHARMAP=${POSTGRES_LANG_CHARMAP}
        image: hoshisakan/bookstore/postgresql:15.2
        container_name: postgresql-dev
        env_file:
          - ./.env
        environment:
            - DATABASE_HOST=${DATABASE_HOST}
            - POSTGRES_USER=${POSTGRES_USER}
            - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
            - POSTGRES_DB=${POSTGRES_DB}
            - PGDATA=/var/lib/postgresql/data
            - TZ=${POSTGRES_TIME_ZONE}
            - POSTGRES_LANG_NAME=${POSTGRES_LANG_NAME}
        volumes:
            - ./data/postgresql/pgdata:/var/lib/postgresql/data
            - ./data/postgresql/pgdata_backup:${POSTGRES_DATA_BACKUP_PATH}
        ports:
            - 5432:${POSTGRES_PORT}
        networks:
            bookstore_common_net:
                ipv4_address: ${POSTGRES_HOST_IP}
        restart: on-failure:3

    pgadmin:
        build: ./conf/postgresql_admin
        image: hoshisakan/bookstore/pgadmin:6.21
        container_name: postgresql-admin-dev
        env_file:
          - ./.env
        environment:
            - PGADMIN_DEFAULT_EMAIL=${PGADMIN_DEFAULT_EMAIL:-test@test.com}
            - PGADMIN_DEFAULT_PASSWORD=${PGADMIN_DEFAULT_PASSWORD:-test123!}
        volumes:
            - pgadmin:/var/lib/pgadmin
            - ./data/pgadmin/pgadmin_data:/var/lib/pgadmin
        ports:
            - 5433:80
        depends_on:
            - postgres_db
        networks:
          bookstore_common_net:
              ipv4_address: ${PGADMIN_HOST_IP}
        user: root
        restart: on-failure:2

networks:
    bookstore_common_net:
        ipam:
          config:
            - subnet: ${NETWORK_SUBNET}
              gateway: ${NETWORK_GATEWAY}

volumes:
    pgadmin:
```

* Use the docker-compose command to build the container
```
docker-compose up -d
```