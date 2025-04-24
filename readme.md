New Sustainable Hospital, C#
=========
A new implementation of Sustainable hospital, this program merges Excel documents, storing products used by different cantinas at various hospitals, with the purpose of tracking Carbon emissions.



The program has a number of modes:

In **merge** mode the program attempts to correctly identify what category each product in any number of supplied Excel belongs to, and create an Excel document with all products of these and all previous documents belong to; the program may ask the user for clarification in some cases.

In **retrain** mode the program re-learns how to identify the categories, based on an Excel file with categories, ingredients, and keywords.

Note, the product is developed for a Danish customer, and thus the products and categories in the example Excel documents, and all printed text from the program are in Danish.

Notice while under development
-----
When under development, the program uses commandline interface; this will be changed before final
