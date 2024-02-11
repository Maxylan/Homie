# (c) 2024 @Maxylan
from typing import Optional
from sqlmodel import SQLModel, Field

"""
The `Attachment` model is a representation of the attachments table in the database.

It represents a single attachment uploaded to Homie.
"""
class Attachment(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    file: str = Field(max_length=255)
    type: str = Field(max_length=63)
    path: Optional[str] = Field(max_length=255)
    source: Optional[str] = Field(max_length=255)
    alt: str = Field(default='', max_length=255)
    timestamp: str = Field(default="CURRENT_TIMESTAMP", not_null=True)

    class Config:
        table_name = "attachments"


"""
GPT 3.5, 2024-02-11: In this representation:

The id column is defined as the primary key.
Constraints like NOT NULL and DEFAULT are provided using the not_null and default parameters within the Field class.
Maximum lengths for string fields are defined using the max_length parameter within the Field class.
The timestamp column is set to automatically use the current timestamp as its default value.
"""