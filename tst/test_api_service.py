import unittest

from src.api_service import ApiService


class ApiServiceTest(unittest.TestCase):
    def setUp(self) -> None:
        self.api_service = ApiService()

    def test_cache_all_messages(self):
        # exercise
        self.api_service.cache_all_messages()

        # post-conditions
        messages = self.api_service.get_cached_messages()
        self.assertEqual(10, len(messages))

    def test_get_labels(self):
        # exercise
        labels = self.api_service.get_labels()

        # post-conditions
        self.assertEqual(20, len(labels))

    def test_get_messages_inbox(self):
        # exercise
        messages = self.api_service.get_messages()

        # post-conditions
        self.assertEqual(10, len(messages))
        self.assertTrue(all('INBOX' in m.labels for m in messages))
        self.assertFalse(any('SENT' in m.labels for m in messages))

    def test_get_messages_sent(self):
        # set-up
        self.api_service.filter_context.label_ids = ['SENT']

        # exercise
        messages = self.api_service.get_messages()

        # post-conditions
        self.assertEqual(10, len(messages))
        self.assertTrue(all('SENT' in m.labels for m in messages))
        self.assertFalse(any('INBOX' in m.labels for m in messages))


if __name__ == '__main__':
    unittest.main()
