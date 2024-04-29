#!/bin/bash
docker stop bookstore_web bookstore_nginx
docker container rm bookstore_web bookstore_nginx
docker image rm hoshisakan/dotnet:6 hoshisakan/bookstore_nginx
docker compose up -d --build