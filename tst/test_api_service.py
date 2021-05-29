import unittest

from src.api_service import ApiService

class ApiServiceTest(unittest.TestCase):
    def setUp(self) -> None:
        self.api_service = ApiService()

    def test_get_labels(self):
        # exercise
        labels = self.api_service.get_labels()

        # post-conditions
        self.assertEqual(20, len(labels))

    def test_get_messages(self):
        # exercise
        messages = self.api_service.get_messages()

        # post-conditions
        self.assertEqual(10, len(messages))
    
if __name__ == '__main__':
    unittest.main()