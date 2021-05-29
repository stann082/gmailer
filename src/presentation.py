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

    def show_messages(self):
        messages = self.service.get_messages()
        output = ""

        counter = 0
        for message in messages:
            labels = ", ".join(message.labels)
            counter += 1
            output += f"Email #{counter}\n"
            output += f"Subject: {message.subject}\n"
            output += f"Date: {message.date}\n"
            output += f"From: {message.sender}\n"
            output += f"To: {message.recepient}\n"
            output += f"Labels: {labels}\n\n"

        print(output)
