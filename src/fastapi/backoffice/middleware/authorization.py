# (c) 2024 @Maxylan

# TODO: I want to ensure that the requesting user both belongs to the platform
# that's being requested, and that requests against potentially destructive 
# actions that requires elevated privileges within a Platform include a valid
# password.
#
# This middleware needs to process the request before routing.