using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Collections;
using System.IO;
using System.Xml;

//Author: $Aleksander Wind$
namespace RSA_encryption
{
    public class RSA_keygen
    {
        public BigInteger n;
        public BigInteger p;
        public BigInteger q;
        public BigInteger totient;
        public int e = 65537;
        public BigInteger d;
        public Random rd = new Random();
        public int iterations = 1024;
        public int interval = 0;
        public static List<KeyPair> keyPairs = new List<KeyPair>();

        public RSA_keygen()
        {
            String user = Environment.UserName;
            String[] files = Directory.GetFiles(@$"C:\\Users\\{user}\\Documents\\RSA_keys");
            foreach (String file in files)
            {
                String keyName = null; ;
                String nStr = null;
                String eStr = null;
                String dStr = null;
                XmlReader reader = XmlReader.Create(file);
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name.ToString())
                        {
                            case "name":
                                keyName = reader.ReadString();
                                break;
                            case "n":
                                nStr = reader.ReadString();
                                break;
                            case "e":
                                eStr = reader.ReadString();
                                break;
                            case "d":
                                dStr = reader.ReadString();
                                break;
                        }
                    }
                }
                KeyPair kp = new KeyPair(keyName, nStr, eStr, dStr);
                keyPairs.Add(kp);
            }
        }
        #region keys
        public void getInput()
        {
            String keyLengthStr;
            int keyLength = 0;
            String name;
            Console.WriteLine("Please the desired length of the key. Only choose 256, 512, 1024 or 2048 bits");
            keyLengthStr = Console.ReadLine();
            try
            {
                keyLength = int.Parse(keyLengthStr);
                if (!(keyLength == 256 || keyLength == 512 || keyLength == 1024 || keyLength == 2048))
                {
                    throw new InvalidDataException();
                }
            }
            catch (InvalidDataException)
            {
                Console.WriteLine("Only choose 256, 512, 1024 or 2048 bits Please try again");
                getInput();
            }
            catch (Exception)
            {
                Console.WriteLine("Illegal input. Please try again");
                Console.ReadKey();
                Console.Clear();
                getInput();
            }

            Console.WriteLine("Please enter a name for your keypair:");
            name = Console.ReadLine();
            calculationStart(keyLength);
            createKeys(name, n, e, d);
        }

        public void calculationStart(int keyLength)
        {
            n = nCalc(keyLength);
            totient = tot(p, q);
            if (!gcd(totient))
            {
                n = nCalc(keyLength);
            }
            d = mmi((BigInteger)e, totient);
            if ((d == 0) || (bitLength(n) < keyLength))
            {
                calculationStart(keyLength);
            }
        }

        #region calculation of n
        public BigInteger nCalc(int keyLength)
        {
            BigInteger maxValSqrt = BigInteger.Pow(2, keyLength / 2);
            BigInteger minValSqrt = BigInteger.Pow(2, (keyLength - 1) / 2)+1;

            double diff = (double)(maxValSqrt - minValSqrt);

            double firstVal = diff * rd.NextDouble();
            double secondVal = diff * rd.NextDouble();

            //Starts tracking of computing time
            DateTime start = DateTime.Now;

            p = primeCalc(firstVal, minValSqrt, maxValSqrt);
            q = primeCalc(secondVal, minValSqrt, maxValSqrt);

            BigInteger nloc = BigInteger.Multiply(p, q);

            //End tracking of computing time
            interval = (int)(DateTime.Now - start).TotalMilliseconds;

            if (!sizeCheck(nloc, keyLength))
            {
                nCalc(keyLength);
            }

            return nloc;
        }

        public BigInteger primeCalc(double val, BigInteger minValSqrt, BigInteger maxValsqrt)
        {
            BigInteger stVal = minValSqrt + (BigInteger)val;

            if (stVal % 2 == 0)
            {
                stVal++;
            }
            for (BigInteger bi = stVal; bi < maxValsqrt; bi += 2)
            {
                if (isPrime(bi))
                {
                    return bi;
                }
            }
            return 0;
        }

        public bool isPrime(BigInteger num)
        {
            BigInteger d = num - 1;

            if (smallPrimeCheck(num))
            {
                while (d % 2 == 0)
                {
                    d /= 2;
                }

                for (int i = 0; i < iterations; i++)
                {
                    if (millerRabinTest(d, num) == false)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool smallPrimeCheck(BigInteger num)
        {
            int[] smallPrimes = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101 };
            foreach (int i in smallPrimes)
            {
                if (num % i == 0)
                {
                    return false;
                }
            }
            return true;
        }

        //GeeksForGeeks 2022
        public bool millerRabinTest(BigInteger d, BigInteger n)
        {
            BigInteger a = (BigInteger)(2 + (rd.Next() % (n - 4)));

            BigInteger x = BigInteger.ModPow(a, d, n);

            if (x == 1 || x == n - 1)
            {
                return true;
            }

            while (d != n - 1)
            {
                x = (x * x) % n;
                d *= 2;

                if (x == 1)
                {
                    return false;
                }
                if (x == n - 1)
                {
                    return true;
                }
            }
            return false;
        }

        public bool sizeCheck(BigInteger n, int keyLength)
        {
            BigInteger uLim = BigInteger.Pow(2, keyLength) - 1;
            BigInteger lLim = BigInteger.Pow(2, keyLength - 1);
            if ((uLim > n) && (n > lLim))
            {
                return true;
            }
            return false;
        }

        public long bitLength(BigInteger bi)
        {
            long bits = 1;

            while ((bi / 2) > 0)
            {
                bi /= 2;
                bits++;
            }
            return bits;
        }
        #endregion

        public BigInteger tot(BigInteger p, BigInteger q)
        {
            return BigInteger.Multiply(p - 1, q - 1);
        }

        public bool gcd(BigInteger totient)
        {
            BigInteger ebi = e;
            BigInteger temp;
            while (ebi != 0)
            {
                temp = ebi;
                ebi = totient % ebi;
                totient = temp;
            }
            return totient == 1;
        }

        //mmi: Modular multiplicative inverse
        public BigInteger mmi(BigInteger e, BigInteger totient)
        {
            BigInteger d = 0;
            BigInteger r = totient;
            BigInteger d1 = 1;
            BigInteger r1 = e;
            BigInteger quot;
            BigInteger temp;
            while (r1 != 0)
            {
                quot = r / r1;
                temp = d1;
                d1 = d - quot * d1;
                d = temp;

                temp = r1;
                r1 = r - quot * r1;
                r = temp;
            }

            if (r > 1)
            {
                return 0;
            }

            if (d < 0)
            {
                d += totient;
            }
            return d;
        }

        public void printKeys()
        {
            int index = 0;
            int selection = -1;
            String[] contents = new String[] { "Empty" };
            foreach (KeyPair kp in keyPairs)
            {
                index++;
                Console.WriteLine(index + " " + kp.ToString());
            }
            Console.WriteLine("Please choose a key to view");
            try
            {
                selection = int.Parse(Console.ReadLine());
            }
            catch (Exception)
            {
                Console.WriteLine("Write only numbers. Please try again");
                printKeys();
            }
            try
            {
                contents = keyPairs[selection - 1].getKeys();
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine($"No keypair with index {selection}");
            }
            String pub = "--------------------public key--------------------";
            String ePub = "------------------end public key------------------";
            String priv = "--------------------private key-------------------";
            String ePriv = "------------------end private key-----------------";
            String[] toPrint = { pub, "n:", contents[0], "e:", contents[1], ePub, priv, contents[2], ePriv };
            foreach (String line in toPrint)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();
        }

        public void createKeys(String name, BigInteger n, BigInteger e, BigInteger d)
        {
            KeyPair kp = new(name, n, e, d);
            keyPairs.Add(kp);

            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.NewLineOnAttributes = true;
            writerSettings.Indent = true;
            String user = Environment.UserName;
            String dir = (@$"C:\\Users\\{user}\\Documents\\RSA_keys");
            XmlWriter xmlWriter = XmlWriter.Create(@$"C:\\Users\\{user}\\Documents\\RSA_keys\\{name}.xml", writerSettings);
            xmlWriter.WriteStartElement("Keys");
            xmlWriter.WriteElementString("name", name);
            xmlWriter.WriteStartElement("Key");
            xmlWriter.WriteAttributeString("privacy", "public");
            xmlWriter.WriteElementString("n", kp.getKeys()[0]);
            xmlWriter.WriteElementString("e", kp.getKeys()[1]);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Key");
            xmlWriter.WriteAttributeString("privacy", "private");
            xmlWriter.WriteElementString("d", kp.getKeys()[2]);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();
            xmlWriter.Close();

            Console.WriteLine();
            Console.WriteLine($"Key generated sucessfully. Total prime computation time: {interval} ms");
            Console.WriteLine();
        }
        #endregion

        #region encryption
        public void encryptionMenu(bool enc)
        {
            Console.WriteLine("Load encryption/decryption key from file or type it into the console?");
            Console.WriteLine("1. File");
            Console.WriteLine("2. Enter as plaintext");
            String input = Console.ReadLine();
            String e = null;
            String n = null;
            String d = null;
            if (enc)
            {
                if (input.Equals("1"))
                {
                    KeyPair kp = loadKeyFromFile();
                    n = kp.getKeys()[0];
                    e = kp.getKeys()[1];
                    d = kp.getKeys()[2];
                }
                else if (input.Equals("2"))
                {
                    Console.WriteLine("Please type in the e value in Base64 format:");
                    e = Console.ReadLine();
                    Console.WriteLine("Please type in the n value in Base64 format:");
                    n = Console.ReadLine();
                }
                encryption(n, e/*, d*/);
            }
            else
            {
                if (input.Equals("1"))
                {
                    KeyPair kp = loadKeyFromFile();
                    n = kp.getKeys()[0];
                    d = kp.getKeys()[2];
                }
                else if (input.Equals("2"))
                {
                    Console.WriteLine("Please type in the d value in Base64 format:");
                    d = Console.ReadLine();
                    Console.WriteLine("Please type in the n value in Base64 format:");
                    n = Console.ReadLine();
                }
                decryption(d, n);
            }
        }

        public KeyPair loadKeyFromFile()
        {
            Console.WriteLine();
            Console.WriteLine("Please select the key:");
            int index = 1;
            KeyPair keyPair = null;
            foreach (KeyPair kp in keyPairs)
            {
                Console.WriteLine($"{index}. {kp.ToString()}");
                index++;
            }
            int input = 0;
            try
            {
                input = int.Parse(Console.ReadLine());
            }
            catch (Exception)
            {
                Console.WriteLine("Please type only numbers. Try again");
                Console.WriteLine();
                return loadKeyFromFile();
            }
            Console.WriteLine();
            try
            {
                if (keyPairs.Count == 0)
                {
                    throw new ArgumentNullException();
                }
                return keyPair = keyPairs[input - 1];
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Illegal value chosen. Please try again");
                Console.WriteLine();
                return loadKeyFromFile();
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("No keys exist yet");
            }
            return null;
        }

        public void encryption(String n, String e/*, String d*/)
        {
            Console.WriteLine();
            Console.WriteLine("Please type in the message that should be encrypted.");
            Console.WriteLine("The messagesize must NOT exceed the bytesize of n-1");
            Console.WriteLine();
            String pltxt = Console.ReadLine();
            BigInteger ptbi = new BigInteger(Encoding.Default.GetBytes(pltxt));
            BigInteger nbi = new BigInteger(Convert.FromBase64String(n));
            BigInteger ebi = new BigInteger(Convert.FromBase64String(e));
            BigInteger cbi = BigInteger.ModPow(ptbi, ebi, nbi);
            String c = Convert.ToBase64String(cbi.ToByteArray());
            Console.WriteLine();
            Console.WriteLine("The encoded message in Base64 format is:");
            Console.WriteLine();
            Console.WriteLine(c);
            #region commented_code
            /*
            Console.WriteLine("c bytes:" + PrintByteArray(Encoding.Default.GetBytes(c)));
            Console.WriteLine("cbi  bytes:" + PrintByteArray(cbi.ToByteArray()));
            BigInteger cbi2 = new BigInteger(Convert.FromBase64String(c));
            Console.WriteLine("cbi2 bytes:" + PrintByteArray(cbi2.ToByteArray()));
            BigInteger dbi2 = new BigInteger(Convert.FromBase64String(d));
            BigInteger nbi2 = new BigInteger(Convert.FromBase64String(n));
            BigInteger ptbi2 = BigInteger.ModPow(cbi, dbi2, nbi);
            Console.WriteLine("ptbi   bytes:" + PrintByteArray(ptbi.ToByteArray()));
            Console.WriteLine("ptbi2  bytes:" + PrintByteArray(ptbi2.ToByteArray()));
            Console.WriteLine("nbi  bytes:" + PrintByteArray(nbi.ToByteArray()));
            String pltxt2 = Encoding.Default.GetString(ptbi2.ToByteArray());
            Console.WriteLine("The decoded message is:");
            Console.WriteLine();
            Console.WriteLine(pltxt2);
            Console.WriteLine();
            */
            #endregion
        }

        public void decryption(String d, String n)
        {
            Console.WriteLine();
            Console.WriteLine("Please type in the message that should be decrypted in Base64 format.");
            Console.WriteLine();
            String c = Console.ReadLine();
            //Console.WriteLine("Get bytes:" + PrintByteArray(Encoding.UTF8.GetBytes(c)));
            BigInteger cbi = new BigInteger(Convert.FromBase64String(c));
            BigInteger dbi = new BigInteger(Convert.FromBase64String(d));
            BigInteger nbi = new BigInteger(Convert.FromBase64String(n));
            BigInteger ptbi = BigInteger.ModPow(cbi, dbi, nbi);
            String pltxt = Encoding.UTF8.GetString(ptbi.ToByteArray());
            Console.WriteLine();
            Console.WriteLine("The decoded message is:");
            Console.WriteLine();
            Console.WriteLine(pltxt);
            Console.WriteLine();
        }
        public String PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            return sb.ToString();
        }
        #endregion
    }
}
#region sources
/*

GeeksforGeeks, 7/4/2022. Primality Test | Set 3 (Miller–Rabin). 
Retrieved 20/6/2022, from https://www.geeksforgeeks.org/primality-test-set-3-miller-rabin/

*/
#endregion
