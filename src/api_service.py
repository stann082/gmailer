import os.path
from googleapiclient.discovery import build
from google_auth_oauthlib.flow import InstalledAppFlow
from google.auth.transport.requests import Request
from google.oauth2.credentials import Credentials

from src.user_message import UserMessage

SCOPES = ['https://www.googleapis.com/auth/gmail.readonly']
CREDS_FILE = 'etc/credentials.json'
TOKEN_FILE = 'etc/token.json'


class ApiService:

    # region Constructors
    def __init__(self):
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

        self.service = build('gmail', 'v1', credentials=creds)

    # region Public Methods
    def get_labels(self):
        results = self.service.users().labels().list(userId='me').execute()
        return results.get('labels', [])

    def get_messages(self, user_id='me'):
        user_messages = []
        messages = self.service.users().messages().list(
            maxResults=10, userId=user_id).execute().get('messages', [])
        for message in messages:
            tdata = self.service.users().messages().get(
                userId=user_id, id=message['id']).execute()
            user_messages.append(UserMessage(tdata['id'], tdata['payload']))

        return user_messages
