class UserMessage:

    def __init__(self, id, payload) -> None:
        self.id = id
        self.date = self.__getHeaderValue(payload, "Date")
        self.sender = self.__getHeaderValue(payload, "From")
        self.subject = self.__getHeaderValue(payload, "Subject")
        self.recepient = self.__getHeaderValue(payload, "To")

    def __getHeaderValue(self, payload, headerName):
        for header in payload['headers']:
            if header['name'] == headerName:
                return header['value']
