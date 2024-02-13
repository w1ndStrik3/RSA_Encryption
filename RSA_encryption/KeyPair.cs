using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

//Author: $Aleksander Wind$
namespace RSA_encryption
{
    public class KeyPair
    {
        String name;

        //Public
        private String n;
        private String e;

        //Private
        private String d;

        public KeyPair(String name, BigInteger n, BigInteger e, BigInteger d)
        {
            this.name = name;

            //pub
            this.n = Convert.ToBase64String(n.ToByteArray());
            this.e = Convert.ToBase64String(e.ToByteArray());
           
            //priv
            this.d = Convert.ToBase64String(d.ToByteArray());

            //Convert.FromBase64String(this.n)
        }

        public KeyPair(String name, String n, String e, String d)
        {
            this.name = name;

            //pub
            this.n = n;
            this.e = e;

            //priv
            this.d = d;
        }

        override
        public String ToString()
        {
            return name;
        }

        public String[] getKeys()
        {
            String[] contents = {n, e, d};
            return contents;
        }
    }
}

