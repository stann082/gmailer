import json
import os.path
import redis

from googleapiclient.discovery import build
from google_auth_oauthlib.flow import InstalledAppFlow
from google.auth.transport.requests import Request
from google.oauth2.credentials import Credentials

from src.filter_context import FilterContext
from src.user_message import UserMessage

SCOPES = ['https://www.googleapis.com/auth/gmail.readonly']
CREDS_FILE = 'etc/credentials.json'
TOKEN_FILE = 'etc/token.json'


class ApiService:

    # region Constructors
    def __init__(self, database=1):
        creds = None
        if os.path.exists(TOKEN_FILE):
            creds = Credentials.from_authorized_user_file(TOKEN_FILE, SCOPES)

        if not creds or not creds.valid:
            if creds and creds.expired and creds.refresh_token:
                creds.refresh(Request())
            else:
                flow = InstalledAppFlow.from_client_secrets_file(
                    CREDS_FILE, SCOPES)
                creds = flow.run_local_server(port=0)
            with open(TOKEN_FILE, 'w') as token:
                token.write(creds.to_json())

        self.api_service = build('gmail', 'v1', credentials=creds)
        self.filter_context = FilterContext()

        self.redis_service = redis.Redis(db=database)

    # region Public Methods
    def cache_all_messages(self):
        for message in self.__get_messages():
            data = self.__get_message_data(message)
            user_message = UserMessage()
            user_message.copyFrom(data)
            key = data['id']
            value = user_message.to_json()
            self.redis_service.set(key, value)

    def clear(self):
        self.redis_service.flushdb()

    def get_cached_messages(self):
        return self.redis_service.keys()

    def get_labels(self):
        results = self.api_service.users().labels().list(
            userId=self.filter_context.user_id
        ).execute()

        return results.get('labels', [])

    def get_messages(self):
        user_messages = []

        for message in self.__get_messages():
            data = self.api_service.users().messages().get(
                userId=self.filter_context.user_id,
                id=message['id']
            ).execute()

            user_message = UserMessage()
            user_message.copyFrom(data)
            user_messages.append(user_message)

        return user_messages

    def search(self, query):
        user_messages = []

        keys = self.get_cached_messages()
        if (len(keys) == 0):
            print("Loading all the emails...")
            self.cache_all_messages()

        keys = self.get_cached_messages()
        for key in keys:
            value = self.redis_service.get(key)
            user_message = UserMessage(value)
            if user_message.matches(query):
                user_messages.append(user_message)

        return user_messages

    # Helper Methods
    def __get_message_data(self, message):
        data = self.api_service.users().messages().get(
            userId=self.filter_context.user_id,
            id=message['id']
        ).execute()
        return data

    def __get_messages(self):
        messages = self.api_service.users().messages().list(
            maxResults=self.filter_context.max_results,
            userId=self.filter_context.user_id,
            labelIds=self.filter_context.label_ids
        ).execute().get('messages', [])

        return messages
