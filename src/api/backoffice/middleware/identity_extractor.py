# (c) 2024 @Maxylan

# TODO: I want to extract the platform and user identity. 
# Primarily from the request headers if possible, URI Path or query string if not.
# Headers:
# "x-requesting-platform" - Platform
# "x-requesting-uid" - ID of the requesting User's Identity / token.
#
# This middleware needs to process the request before routing and before the 
# "auth" middleware (authorization.py).