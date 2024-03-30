import os

from models.homiedb import *
from sqlmodel import SQLModel, create_engine
from sqlalchemy.orm import sessionmaker

IS_DEVELOPMENT = os.environ.get('FASTAPI_IS_DEVELOPMENT', True)
NICENAME = os.environ.get('FASTAPI_NAME')
FASTAPI_NAME = os.environ.get('FASTAPI_NAME')
FASTAPI_HOST = os.environ.get('FASTAPI_HOST')
FASTAPI_PORT = os.environ.get('FASTAPI_PORT')

# Assert that "api" variables exists.
assert NICENAME, 'FASTAPI_NAME is not set'
assert FASTAPI_NAME, 'FASTAPI_NAME is not set'
assert FASTAPI_HOST, 'FASTAPI_HOST is not set'
assert FASTAPI_PORT, 'FASTAPI_PORT is not set'

DB_DRIVER = os.environ.get('DB_DRIVER', 'mysql')
DB_USER = os.environ.get('DB_USER')
DB_PASSWORD = os.environ.get('DB_PASSWORD')
DB_HOST = os.environ.get('DB_HOST')
DB_PORT = os.environ.get('DB_PORT')
DB_SCHEMA = os.environ.get('DB_SCHEMA', 'HomieDB')
log_sql = os.environ.get('DB_LOG_SQL', IS_DEVELOPMENT)

# Assert that "database" variables exists.
assert DB_USER, 'DB_USER is not set'
assert DB_PASSWORD, 'DB_PASSWORD is not set'
assert DB_HOST, 'DB_HOST is not set'
assert DB_PORT, 'DB_PORT is not set'

engine = create_engine(f'{DB_DRIVER}://{DB_USER}:{DB_PASSWORD}@{DB_HOST}:{DB_PORT}/{DB_SCHEMA}', echo = log_sql)
SQLModel.metadata.create_all(engine)
Session = sessionmaker(engine)
