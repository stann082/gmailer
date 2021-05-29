import json


class UserMessage:

    def __init__(self, json_value = None):
        if json_value is None:
            return

        s = json.loads(json_value)
        self.date = None if 'date' not in s else s['date']
        self.id = None if 'id' not in s else s['id']
        self.labels = None if 'labels' not in s else s['labels']
        self.recepient = None if 'recepient' not in s else s['recepient']
        self.sender = None if 'sender' not in s else s['sender']
        self.subject = None if 'subject' not in s else s['subject']

    def copyFrom(self, data):
        self.id = data['id']
        self.labels = data['labelIds']

        payload = data['payload']
        self.date = self.__get_header_value(payload, "Date")
        self.sender = self.__get_header_value(payload, "From")
        self.subject = self.__get_header_value(payload, "Subject")
        self.recepient = self.__get_header_value(payload, "To")
        pass

    def matches(self, query):
        return (self.__contains(self.date, query)
            or self.__contains(self.sender, query)
            or self.__contains(self.subject, query)
            or self.__contains(self.recepient, query))

    def to_json(self):
        return json.dumps(self, default=lambda o: o.__dict__, sort_keys=True, indent=4)

    def __contains(self, property, query):
        propertyValue = str(property).lower()
        queryValue = str(query).lower()
        return queryValue in propertyValue

    def __get_header_value(self, payload, header_name):
        for header in payload['headers']:
            if header['name'] == header_name:
                return header['value']
