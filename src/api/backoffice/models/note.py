# (c) 2024 @Maxylan
from typing import Optional
from sqlmodel import SQLModel, Field, ForeignKey

class Note(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    slug: str = Field(max_length=255, not_null=True)
    platform_id: int = Field(foreign_key="platforms.id", not_null=True)
    title: str = Field(max_length=255, not_null=True)
    color: Optional[str] = Field(max_length=63, default=None)
    data: Optional[str] = None

    class Config:
        table_name = "notes"
        indexes = [("title_index", ["title", "slug"])]

"""
GPT 3.5, 2024-02-11: In this representation:

The notes table includes various constraints such as NOT NULL and foreign key relationships.
An index is established for efficient querying based on title and slug.
Foreign key constraints are established using the foreign_key parameter within the Field class.
"""