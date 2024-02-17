import os

from models.homiedb import *
from sqlmodel import SQLModel, create_engine
from sqlalchemy.orm import sessionmaker

IS_DEVELOPMENT = os.environ.get('API_IS_DEVELOPMENT', True)
NICENAME = os.environ.get('API_NAME')
API_NAME = os.environ.get('API_NAME')
API_HOST = os.environ.get('API_HOST')
API_PORT = os.environ.get('API_PORT')

# Assert that "api" variables exists.
assert NICENAME, 'API_NAME is not set'
assert API_NAME, 'API_NAME is not set'
assert API_HOST, 'API_HOST is not set'
assert API_PORT, 'API_PORT is not set'

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
