from __future__ import print_function
from src.api_service import ApiService


class Presentation:

    # region Constructors
    def __init__(self):
        self.service = ApiService()

    # region Public Methods
    def show_labels(self):
        labels = self.service.get_labels()
        if not labels:
            print('No labels found.')
        else:
            print('Labels:')
            for label in labels:
                print(label['name'])

    def show_messages(self, user_id='me'):
        messages = self.service.get_messages()
        output = ""

        counter = 0
        for message in messages:
            for header in message['headers']:
                if header['name'] == 'Date':
                    date_value = header['value']
                if header['name'] == 'From':
                    from_value = header['value']
                if header['name'] == 'Subject':
                    subject = header['value']
                if header['name'] == 'To':
                    to_value = header['value']

            counter += 1
            output += f"Email #{counter}\n"
            output += f"Subject: {subject}\n"
            output += f"Date: {date_value}\n"
            output += f"From: {from_value}\n"
            output += f"To: {to_value}\n\n"

        print(output)
