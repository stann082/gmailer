import json


class UserMessage:

    def __init__(self, data) -> None:
        self.id = data['id']
        self.labels = data['labelIds']

        payload = data['payload']
        self.date = self.__getHeaderValue(payload, "Date")
        self.sender = self.__getHeaderValue(payload, "From")
        self.subject = self.__getHeaderValue(payload, "Subject")
        self.recepient = self.__getHeaderValue(payload, "To")

    def to_json(self):
        return json.dumps(self, default=lambda o: o.__dict__, sort_keys=True, indent=4)

    def __getHeaderValue(self, payload, headerName):
        for header in payload['headers']:
            if header['name'] == headerName:
                return header['value']
