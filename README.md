# Homie

*Under construction. I've got a dayjob, man, who's got time to write all this.*

In a perfect world, this will be our all-purpose home-life convenience app. Combining note-taking, shopping-list creating, message-pinning, recipes + meal planning, and more!

It's also just a hobby project I don't intend to spread beyond a closed circle of friends and family *(If I manage to complete it)*. However, anyone is free to publish this in the cloud. It's infrastructure is designed to be able to support widespread use by many people. Thanks to tying all data in [HomieDB]() to *["Platforms"]()*.

If you're not already in it, you can find the [Repository](https://github.com/Maxylan/Homie) [^here]!

- [Features](#features)
- [README](#readme)
- [Structure](#structure)
- [Design Notes](#design-Notes)
- [Technical Notes](#technical-Notes)
- [References](#references)

# Features

*Under construction.*

### Reverse Proxy / Logging

### Dashboard

### Notes

### Shopping List

### Recipe Book

### Recipe Picker

### Pins / Pinned Messages

### Reminders / Scheduler

### Settings

# README

*Under construction.*

# Structure

### HomieGate

This is a **[Docker Compose](https://docs.docker.com/compose/)** project where I've "hid" all services besides the [Reverse Proxy](#reverse-proxy-/-logging) by hosting them on different ports that won't be open to the public. The idea behind this is three fold: 

- It gives me as a developer greater **control and insight** over the whole ecosystem, by having all traffic flow through a singular point that I can monitor and control.
- Instead of having to implement things like routing, "access" & "error" logging and [TSL/SSL]() Conversion across all services, **I can save a lot of time** by letting the flexible and easily-extendible [Scala]() [Reverse Proxy](#reverse-proxy-/-logging) do all of of these things up-front. 
- Between all traffic flowing through the singular point of the [Reverse Proxy](#reverse-proxy-/-logging), and the ease of regulating what calls should be allowed to the different services, it adds several **layers of security**.

Behind the Reverse Proxy, the rest of my services lay;
- **[Homie](#homieapp)** - [Flutter App]() - The main attraction.
- **[HomieWeb](#homieweb)** - [Apache2 / httpd]() - Enables desktop access.
- **[HomieBackoffice](#homiebackoffice)** - [Python Uvicorn & FastAPI]() - RESTful Backend
- **[HomieDB](#homiedb)** - [MySQL Database]() - Stores bits and bobs

### HomieApp

### HomieWeb

### HomieBackoffice

### HomieDB

# Design Notes

*Under construction.*

# Technical Notes

*Under construction.*


---
# References

https://github.com/Maxylan/Homie

[^here]: https://github.com/Maxylan/Homie