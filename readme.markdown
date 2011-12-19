Introduction
===============================================================================
This is a fork of nDumbster, the fake SMTP server, that was originally hosted at: http://ndumbster.sourceforge.net/default.html. nDumbster is useful in automated tests where instead of having to create a wrapper around the mail sending then providing a mock
implementation you actually test the email sending component. The SMTP server keeps the messages in memory so you can query what has been sent.

Enhancements
-------------------------------------------------------------------------------
Apart from being upgraded to .Net 4.0 the following improvements have been made to the library:

    1. The locking issues that lead to slow test times have been removed by using the ConcurrentQueue
    2. Generics have been used instead of ArrayLists
