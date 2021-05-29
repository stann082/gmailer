#!/usr/bin/python3

import argparse
from src.presentation import Presentation


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "-l", "--labels", help="Show all labels associated with your account", action="store_true")
    parser.add_argument("-m", "--messages", help="Show Inbox messages", action="store_true")
    parser.add_argument("-q", "--query", help="Search for item in messages")
    parser.add_argument("-c", "--clear", help="Clear redis database", action="store_true")
    args = parser.parse_args()

    presentation = Presentation()

    if args.labels:
        presentation.show_labels()
    elif args.messages:
        presentation.show_messages()
    elif args.query:
        presentation.search(args.query)
    elif args.clear:
        presentation.clear()
    else:
        print("No argument is selected. Pass -h or --help for details")


if __name__ == '__main__':
    main()
