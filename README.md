<p align="center">
    <img alt="OpenBudgeteer banner" src="https://github.com/TheAxelander/OpenBudgeteer/blob/master/assets/banner.png?raw=true">
</p>

<p align="center">
    <a href="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/docker-image-pre-release.yml">
        <img alt="Docker Image pre-release" src="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/docker-image-pre-release.yml/badge.svg">
    </a>
    <a href="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/docker-image-master.yml">
        <img alt="Docker Image latest" src="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/docker-image-master.yml/badge.svg">
    </a>
</p>
<p align="center">
    <a href="https://github.com/awesome-selfhosted/awesome-selfhosted#money-budgeting--management">
        <img alt="Mentioned in Awesome-Selfhosted" src="https://awesome.re/mentioned-badge.svg">
    </a>
    <img alt="Docker Pulls" src="https://img.shields.io/docker/pulls/axelander/openbudgeteer">
    <img alt="GitHub release (latest by date)" src="https://img.shields.io/github/v/release/theaxelander/openbudgeteer">
</p>

OpenBudgeteer is a budgeting app based on the Bucket Budgeting Principle and inspired by [YNAB](https://www.youneedabudget.com) and [Buckets](https://www.budgetwithbuckets.com). The Core is based on .NET and the MVVM Pattern, the Front End uses Blazor Server.

![Screenshot 1](assets/screenshot1.png)

--------------------

## Documentation

Within the [Documentation](https://theaxelander.github.io) you will find all the details how to install and setup OpenBudgeteer and how it is used. Some sections are still WIP but you should find the most important things that were previously maintained here in the README.

## Quick Start

For a quick ramp-up up of OpenBudgeteer using Docker and Sqlite use below command or docker compose.

### docker run

```bash
docker run -d --name='openbudgeteer' \
    -e 'CONNECTION_PROVIDER'='SQLITE' \
    -e 'CONNECTION_DATABASE'='/srv/openbudgeteer.db' \
    -p 8080:8080
    -v 'data:/srv'  \
    'axelander/openbudgeteer:pre-release'
```

### docker compose

```yml
version: "3"

services:
  openbudgeteer:
    image: axelander/openbudgeteer:pre-release
    container_name: openbudgeteer
    ports:
      - 8080:8080
    environment:
      - CONNECTION_PROVIDER=SQLITE
      - CONNECTION_DATABASE=/srv/openbudgeteer.db
    volumes:
      - data:/srv
        
volumes:
  data:
```
