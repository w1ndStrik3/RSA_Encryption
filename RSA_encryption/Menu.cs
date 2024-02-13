using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Author: $Aleksander Wind$
namespace RSA_encryption
{
    public class Menu
    {
        RSA_keygen keyGen = new RSA_keygen();
        public Menu()
        {
            menuOptions();
        }
        public void menuOptions()
        {
            while (true)
            {
                int selection = 0;
                Console.WriteLine("Please choose the desired menu option, by typing in it's number:");
                Console.WriteLine();
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. Key generation");
                Console.WriteLine("2. View all keys");
                Console.WriteLine("3. Encryption");
                Console.WriteLine("4. Decryption");

                try
                {
                    selection = int.Parse(Console.ReadLine());
                }
                catch (Exception)
                {
                    Console.WriteLine("Please type only numbers. Try again.");
                    Console.WriteLine();
                    menuOptions();
                }

                switch (selection)
                {
                    case 0:
                        Environment.Exit(69);
                        break;
                    case 1:
                        Console.Clear();
                        keyGen.getInput();
                        break;
                    case 2:
                        Console.Clear();
                        keyGen.printKeys();
                        break;
                    case 3:
                        keyGen.encryptionMenu(true);
                        break;
                    case 4:
                        keyGen.encryptionMenu(false);
                        break;
                    default:
                        Console.WriteLine("Invalid menu option chosen. Please try again");
			Console.WriteLine();
                        menuOptions();
                        break;
                }

            }
        }
    }
}
