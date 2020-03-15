
## Introduction

The RaceMonitorConsole application takes raw telemetry data is arriving via MQTT and provides an event stream and car status information such as speed and position for the front-end application to display.
It enhances the data presented to visualise F1 cars going around a track. 

## Prerequisites

Ensure docker is running Linux containers.

* Start the McLaren MAT components
```console
$ cd c:\work\Gillian\McLaren\MAT-Coding-Challenge-master\
$ docker-compose pull
$ docker-compose up -d
Creating network "mat-coding-challenge_default" with the default driver
Creating broker ... done
Creating source_gps        ... done
Creating mqtt-to-websocket ... done
Creating webapp            ... done
```

Open (http://localhost:8084)

## Instructions - how to build and run the code

Download the code from https://github.com/gillianshome/RaceMonitorConsole

Open the RaceMonitorConsole.sln in Visual Studio and run the RaceMonitorConsole project.
The event stream and car status information appears on the McLaren Dashboard.

## Instructions - running in a container 

Ideally this application would run with the McLaren MAT components, I have provided a modified docker-compose.yaml intended to add the RaceMonitorConsole application to the container running the McLaren MAT components and to link the racemonitorconsole to the broker. It should replace the file in the MAT-Coding-Challenge-master folder. 
However the event stream and car status information does not appear on the McLaren Dashboard even though the connection status indicators shows connected (green).

## Other investigations - build and run an image

* Build the project using the Dockerfile contained in it and run the resulting application.

```console
$ cd RaceMonitorConsole
$ docker build -t racemonitorconsole .
$ docker run --name racemonitorconsole racemonitorconsole:latest
```

The running race monitor application cannot establish network connections between the MQTT broker and either the MQTT publisher or subscriber clients. They are in a different container.

* In Visual Studio select the RaceMonitorConsole\Docker file and select 'Build Docker Image'

* Extract and run the application

```console
$ docker cp monitor:/app/RaceMonitorConsole.dll .
$ dotnet .\RaceMonitorConsole.dll

```
