# (c) 2024 @Maxylan

# TODO: I want to extract the platform and user identity. 
# Primarily from the request headers if possible, URI Path or query string if not.
# Headers:
# "x-requesting-platform" - Platform
# "x-requesting-uid" - ID of the requesting User's Identity / token.
#
# This middleware needs to process the request before routing and before the 
# "auth" middleware (authorization.py).

from fastapi import FastAPI, Request
from main import homie

public_routes = [
    "/",
    "/platform/join/*",
    "/healthcheck",
    "/favicon.ico",
    "/robots.txt",
    "/docs",
    "/redoc",
    "/openapi.json"
]

@homie.middleware("http")
async def identity_extractor(request: Request, call_next):
    if request.url.path in public_routes:
        response = await call_next(request)
        return response
    
    if (request.headers["x-requesting-platform"] == None):
        if "platform" in request.query_params:
            request.headers["x-requesting-platform"] = request.query_params["platform"]
        else:
            
    
    response = await call_next(request)
    return response