<p style="text-align: center">
    <img alt="OpenBudgeteer banner" src="https://github.com/TheAxelander/OpenBudgeteer/blob/master/assets/banner.png?raw=true">
</p>

<p style="text-align: center">
    <a href="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/test-cases.yml">
        <img alt="Test Cases" src="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/test-cases.yml/badge.svg">
    </a>
    <a href="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/docker-image-pre-release.yml">
        <img alt="Test Cases" src="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/docker-image-pre-release.yml/badge.svg">
    </a>
    <a href="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/docker-image-master.yml">
        <img alt="Test Cases" src="https://github.com/TheAxelander/OpenBudgeteer/actions/workflows/docker-image-master.yml/badge.svg">
    </a>
</p>
<p style="text-align: center">
    <img alt="Docker Pulls" src="https://img.shields.io/docker/pulls/axelander/openbudgeteer">
    <img alt="GitHub release (latest by date)" src="https://img.shields.io/github/v/release/theaxelander/openbudgeteer">
</p>

OpenBudgeteer is a budgeting app based on the Bucket Budgeting Principle and inspired by [YNAB](https://www.youneedabudget.com) and [Buckets](https://www.budgetwithbuckets.com). The Core is based on .NET and the MVVM Pattern, the Front End uses Blazor Server.

![Screenshot 1](assets/screenshot1.png)

## Installation (Docker)

You can use the pre-built Docker Image from [Docker Hub](https://hub.docker.com/r/axelander/openbudgeteer). It requires a connection to a `MySQL` database which can be achieved by passing the following variables:

| Variable | Description | Example |
| --- | --- | --- |
| Connection:Provider | Type of database that should be used | mysql |
| Connection:Server | IP Address to MySQL Server | 192.168.178.100 |
| Connection:Port| Port to MySQL Server | 3306 |
| Connection:Database | Database name | MyOpenBudgeteerDb |
| Connection:User | Database user | MyOpenBudgeteerUser |
| Connection:Password | Database password | MyOpenBudgeteerPassword |

```bash
docker run -d --name='openbudgeteer' \
    -e 'Connection:Provider'='mysql' \
    -e 'Connection:Server'='192.168.178.100' \
    -e 'Connection:Port'='3306' \
    -e 'Connection:Database'='MyOpenBudgeteerDb' \
    -e 'Connection:User'='MyOpenBudgeteerUser' \
    -e 'Connection:Password'='MyOpenBudgeteerPassword' \
    -p '6100:80/tcp' \
    'axelander/openbudgeteer:latest'
```

Alternatively you can use a local `Sqlite` database using the below settings:

```bash
docker run -d --name='openbudgeteer' \
    -e 'Connection:Provider'='sqlite' \
    -v '/my/local/path:/app/database'  \
    -p '6100:80/tcp' \
    'axelander/openbudgeteer:latest'
```
If you don't change the Port Mapping you can access the App with Port `80`. Otherwise like above example it can be accessed with Port `6100`

### Pre-release Version (Docker)

A Pre-Release version can be used with the Tag `pre-release`

```bash
docker run -d --name='openbudgeteer' \
    -e 'Connection:Provider'='mysql' \
    -e 'Connection:Server'='192.168.178.100' \
    -e 'Connection:Port'='3306' \
    -e 'Connection:Database'='MyOpenBudgeteerDb' \
    -e 'Connection:User'='MyOpenBudgeteerUser' \
    -e 'Connection:Password'='MyOpenBudgeteerPassword' \
    -p '6100:80/tcp' \
    'axelander/openbudgeteer:pre-release'
```

### Docker-Compose

Below an example how to deploy OpenBudgeteer together with MySql Server and phpMyAdmin for administration. Please note that user and database `openbudgeteer` need to be availabe, otherwise the container `openbudgeteer` will not work.

So what you can do this is running below Docker Compose, create user and database using phpMyAdmin and then restart either container `openbudgeteer` or the whole Docker Compose.

```yml
version: "3"

networks:
  app-global:
    external: true
  mysql-internal:


services:
  openbudgeteer:
    image: axelander/openbudgeteer
    container_name: openbudgeteer
    ports:
      - 8081:80
    environment:
      - Connection:Server=openbudgeteer-mysql
      - Connection:Port=3306
      - Connection:Database=openbudgeteer
      - Connection:User=openbudgeteer
      - Connection:Password=openbudgeteer
    depends_on:
      - mysql
    networks:
      - app-global
      - mysql-internal

  mysql:
    image: mysql
    container_name: openbudgeteer-mysql
    environment:
      MYSQL_ROOT_PASSWORD: myRootPassword
    volumes:
      - data:/var/lib/mysql
    networks:
      - mysql-internal

  phpmyadmin:
    image: phpmyadmin/phpmyadmin
    container_name: openbudgeteer-phpmyadmin
    links:
      - mysql:db
    ports:
      - 8080:80
    networks:
      - app-global
      - mysql-internal

volumes:
  data:
```

## How to use

### Create Bank Account

The best way to start with OpenBudgeteer is to create at least on Bank Account on the `Account Page`.

![Screenshot 2](assets/screenshot2.png)

### Import Transactions

After that export some Transactions from your Online Banking and import the data using `Import Page`. At the moment it support CSV files only but you can individually set the characters for delimiter and text qualifier. The respective settings and other options are shown once the CSV file has been uploaded.

![Screenshot 3](assets/screenshot3.png)

You also need to create an initial Transaction which includes the Bank Balance on a certain date. It should be the previous day of the very first imported Transaction. You can do this on the `Transaction Page`.

Example:

You have imported all Transactions starting 2020-01-01. To have the right Balances create a Transaction for 2019-12-31 and add as amount the Account Balance of this day. You can mark this Transaction as `Income` (see more explanation in section `Bucket Assignment`).

![Screenshot 4](assets/screenshot4.png)

### Create Buckets

Once you have some Transactions imported you can start creating Buckets on the `Bucket Page`. If you don't know what kind of Buckets you need, maybe start with some Buckets for your monthly or even yearly expenses like Car Insurance, Property Taxes, Instalments etc. and Buckets for your regular needs like Groceries or Gas. You can also create a Bucket for your next big trip by putting some money into it every month.

If you are happy with your setup, put some money into your Buckets. You can do it manually or automatically if a Bucket has a Want for the current month.

### Bucket Assignment

In the final step you assign your Transactions to certain Buckets. Go back to the `Transaction Page`, edit a Transaction and select an appropiate Bucket. You can also do a mass edit. If a Transaction belongs to more than one Bucket just reduce the assigned amount and you get automatically the option to assign the remaining amount to anthoer Bucket.

![Screenshot 5](assets/screenshot5.png)

Transactions which represent your (monthly) income can be assigned to the pre-defined `Income` Bucket. If you have transffered money from one Account to another you can use the `Transfer` Bucket. Please ensure that all `Transfer` Transaction have in total a 0 Balance to prevent data inconsistency and wrong calculations.

Once all Transactions are assigned properly you can go back to the Bucket Overview to see if your Budget management is still fine or if you need to do some movements. You should always ensure that your Buckets don't have a negative Balance. Also your `Remaining Budget` should never be negative.

### Bucket Histroy

OpenBudgeteer has a built-in versioning for Buckets which enables a proper histroy view on previous months. If you modify a Bucket, like changing the Type or the Target Amount, it will create a new version for the current selected month. It is not recommended to change a Bucket in the past, a change between two Bucket Version is prevented.

If you close a Bucket it will be marked as `Inactive` for the next month. This can be only done if the Bucket Balance is 0 to prevent wrong calculations.

![Screenshot 6](assets/screenshot6.png)
