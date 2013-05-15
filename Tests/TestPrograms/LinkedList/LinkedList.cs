using System;

namespace LinkedList
{
    class LinkedListElement<T>
    {
        public T Data;
        public LinkedListElement<T> Prev;
        public LinkedListElement<T> Next;

        public LinkedListElement(T Data)
        {
            this.Data = Data;
        }
    }

    class LinkedList<T>
    {
        public LinkedListElement<T> Head;
        public LinkedListElement<T> Tail;

        public LinkedList()
        {
            Head = null;
            Tail = null;
        }

        public void Add(T element)
        {
            LinkedListElement<T> Listelement = new LinkedListElement<T>(element);
            if (Head == null)
            {
                Head = Listelement;
            }
            else
            {
                Listelement.Prev = Tail;
                Tail.Next = Listelement;
            }
            Tail = Listelement;
        }

        public LinkedListElement<T> Next(LinkedListElement<T> element)
        {
            if (element == null)
                throw new Exception("null is not a valid argument");
            return element.Next;
        }

        public LinkedListElement<T> Prev(LinkedListElement<T> element)
        {
            if (element == null)
                throw new Exception("null is not a valid argument");
            return element.Prev;
        }

        public void Remove(LinkedListElement<T> element)
        {
            if (element == null)
                throw new Exception("null is not a valid argument");

            if (element == Head)
                Head = element.Next;
            if (element == Tail)
                Tail = element.Prev;

            if (element.Prev != null)
            {
                LinkedListElement<T> prev = element.Prev;
                prev.Next = element.Next;
            }
            if (element.Next != null)
            {
                LinkedListElement<T> next = element.Next;
                next.Prev = element.Prev;
            }
        }

        public LinkedListElement<T> Find(T needle)
        {
            return Find(needle, Head);
        }

        public LinkedListElement<T> Find(T needle, LinkedListElement<T> start)
        {
            LinkedListElement<T> iterator = start;            
            while (iterator != null)
            {                
                if (iterator.Data.Equals(needle)) return iterator;
                iterator = iterator.Next;
            }
            return null;
        }
    }

    class MainClass
    {
        public static void Main(string[] args)
        {
            LinkedList<string> myList = new LinkedList<string>();
            myList.Add("Hello World");
            myList.Add("Good day to you");
            myList.Add("Once upon a time in the west");

            LinkedListElement<string> search_it = myList.Find("Good day to you");

            if (search_it != null)
            {
                Console.WriteLine("Found: {0}", search_it.Data);
                myList.Remove(search_it);
            }
            else
                Console.WriteLine("Not found!");

            // Print the contents of the entire list
            LinkedListElement<string> print_it = myList.Head;

            int Lp = 0;
            while (print_it != null)
            {
                Console.WriteLine("{0} {1}", Lp++, print_it.Data);
                print_it = myList.Next(print_it);
            }

        }
    }
}
