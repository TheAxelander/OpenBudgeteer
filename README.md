<p align="center">
    <img alt="OpenBudgeteer banner" src="https://github.com/TheAxelander/OpenBudgeteer/blob/master/assets/banner.png?raw=true">
</p>

<p align="center">
    <a href="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/docker-image-pre-release.yml" target="_blank"><img alt="Docker Image pre-release" src="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/docker-image-pre-release.yml/badge.svg"></a>
    <a href="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/docker-image-master.yml" target="_blank"><img alt="Docker Image latest" src="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/docker-image-master.yml/badge.svg"></a>
</p>
<p align="center">
    <a href="https://github.com/awesome-selfhosted/awesome-selfhosted#money-budgeting--management" target="_blank"><img alt="Mentioned in Awesome-Selfhosted" src="https://awesome.re/mentioned-badge.svg"></a>
    <img alt="Docker Pulls" src="https://img.shields.io/docker/pulls/axelander/openbudgeteer">
    <img alt="GitHub release (latest by date)" src="https://img.shields.io/github/v/release/theaxelander/openbudgeteer">
</p>

OpenBudgeteer is a budgeting app based on the Bucket Budgeting Principle and inspired by [YNAB](https://www.youneedabudget.com) and [Buckets](https://www.budgetwithbuckets.com). The Core is based on .NET and the MVVM Pattern, the Front End uses Blazor Server.

![Screenshot 1](assets/screenshot1.png)

--------------------

## Documentation

Within the [Documentation](https://theaxelander.github.io) you will find all the details how to install and setup OpenBudgeteer and how it is used. Some sections are still WIP but you should find the most important things that were previously maintained here in the README.

## Quick Start

For a quick ramp-up up of OpenBudgeteer using Docker and MariaDB use below docker compose.

```yml
services:
  openbudgeteer:
    image: axelander/openbudgeteer:latest
    #image: axelander/openbudgeteer:pre-release
    #image: axelander/openbudgeteer:1.7
    container_name: openbudgeteer
    ports:
      - 8080:8080
    environment:
      - CONNECTION_PROVIDER=mariadb
      - CONNECTION_SERVER=openbudgeteer-mysql
      - CONNECTION_PORT=3306
      - CONNECTION_DATABASE=openbudgeteer
      - CONNECTION_USER=openbudgeteer
      - CONNECTION_PASSWORD=openbudgeteer
      - APPSETTINGS_CULTURE=en-US
      - APPSETTINGS_THEME=solar
    depends_on:
      - mariadb
      
  mariadb:
    image: mariadb
    container_name: openbudgeteer-mysql
    environment:
      MYSQL_ROOT_PASSWORD: myRootPassword
    volumes:
      - data:/var/lib/mysql
      
  # optional    
  phpmyadmin:
    image: phpmyadmin/phpmyadmin
    container_name: openbudgeteer-phpmyadmin
    links:
      - mariadb:db
    ports:
      - 8081:80
        
volumes:
  data:
```
