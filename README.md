
## Introduction

The RaceMonitorConsole application takes raw telemetry data arriving via MQTT and provides an event stream and car status information such as speed and position for the front-end application to display.
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

## Learning - running in a container 

Ideally this application would run with the McLaren MAT components, I have provided a modified docker-compose.yaml intended to add the RaceMonitorConsole application to the container running the McLaren MAT components and to link the racemonitorconsole to the broker. It should replace the file in the MAT-Coding-Challenge-master folder. 
However the event stream and car status information does not appear on the McLaren Dashboard even though the connection status indicators show connected (green).

This uses the autobuild feature of Docker Hub that creates a new image when new source is uploaded to GitHub.

## Other investigations - build and run an image

* Build the project using the Dockerfile contained in it and run the resulting application.

```console
$ cd RaceMonitorConsole
$ docker build -t racemonitorconsole .
$ docker run --name racemonitorconsole racemonitorconsole:latest
```

The running race monitor application cannot establish network connections between the MQTT broker and either the MQTT publisher or subscriber clients. They are in a different container.

* In Visual Studio select the RaceMonitorConsole\Docker file and select 'Build Docker Image'

This is the same as calling the "docker build ..." command

## Testing

Simple unit test code is given in RaceTrackUnitTest.txt.

## What I did

First challenge to learn about docker and get the home development environment working, a PC upgrade was needed including the Getting started steps (from https://github.com/McLarenAppliedTechnologies/MAT-Coding-Challenge) and testing the setup with `mosquitto_pub`.

Second write a basic application to handle the external interfaces of the application (the MqttRaceClient class)
* to parse incoming telemetry data
* and publish simple but representative race data (position, speed and event messages)
* this was initially done as a .NET Framework application for easier control for debugging subsequently switched to .NET Core and a different MQTT library with improved reliability

Third process the telemetry data, 
* speed - calculated from distance and time delta of location updates
* position, derived from angle to centre of the track
* events - lap times and overtaking (filtered to prevent flooding of events)

Fourth work out how to publish an image via docker hub and run it from a docker-compose.yaml file.

