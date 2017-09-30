using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StackExchange.Redis;
using MongoDB.Bson;

namespace PaquetesTuristicos.Models
{
    public class Cart
    {
        public String HashKey { get; set; }
        public String FareKey { get; set; }
        public List<Tuple<Service, Fare>> ShoppingCart = new List<Tuple<Service, Fare>>();

        public Cart(int idUser)
        {
            this.HashKey = "cart:" + idUser.ToString();
            this.FareKey = "fare:" + idUser.ToString();
        }

        private Boolean Exists()
        {
            var redis = RedisStore.RedisCache;

            if (redis.KeyExists(HashKey))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void addToCart(string productId, string fareName)
        {
            var redis = RedisStore.RedisCache;
            FareKey += productId;
            if (redis.HashExists(HashKey, productId))
            {
                if (redis.HashExists(FareKey, fareName))
                {
                    redis.HashIncrement(FareKey, fareName);
                }else
                {
                    HashEntry[] FareHash =
                    {
                    new HashEntry(fareName, 1)
                    };
                    redis.HashSet(FareKey, FareHash);
                }
            }
            else
            {
                HashEntry[] cartHash =
                {
                new HashEntry(productId, FareKey)
                };
                redis.HashSet(HashKey, cartHash);

                HashEntry[] FareHash =
                {
                new HashEntry(fareName, 1)
                };
                redis.HashSet(FareKey, FareHash);
            }
        }

        public void remove(string productId, string fare)
        {
            var redis = RedisStore.RedisCache;
            FareKey += productId;
            if (redis.HashExists(HashKey, productId))
            {
                redis.HashDelete(FareKey, fare);
                if (redis.ListLength(FareKey) == 0)
                {
                    redis.HashDelete(HashKey, productId);
                }
                
            }
        }
        
        public void clearCart()
        {
            ShoppingCart.Clear();
            var redis = RedisStore.RedisCache;
            var itemHash = redis.HashGetAll(HashKey);
            foreach (var Item in itemHash)
            {
                redis.KeyDelete((Item.Value).ToString(), CommandFlags.FireAndForget);
            }
            redis.KeyDelete(HashKey, CommandFlags.FireAndForget);
        }

        public void changeQty(string productId, int qty)
        {
            var redis = RedisStore.RedisCache;
            if (redis.HashExists(HashKey, productId))
            {
                redis.HashDelete(HashKey, productId);
                HashEntry[] cartHash =
                {
                new HashEntry(productId, qty)
                };
                redis.HashSet(HashKey, cartHash);
            }
        }

        public int getCartSize()
        {
            var redis = RedisStore.RedisCache;
            //if (Exists())
            //{
            //    var len = redis.Execute("HLEN " + HashKey);
            //    return Int32.Parse(len.ToString());
            //}
            //else
            //{
            //    return -1;  // cart does not exist
            //}
            return Convert.ToInt32(redis.ListLength(HashKey));
        }

        public void loadCartItems()
        {
            ShoppingCart.Clear();
            
            var redis = RedisStore.RedisCache;
            MongoConnect mongo = new MongoConnect();
            if (Exists())
            {
                var itemHash = redis.HashGetAll(HashKey);

                foreach (var Item in itemHash)
                {
                    //FareKey += (Item.Name).ToString();
                    var fareHash = redis.HashGetAll((Item.Value).ToString());

                    Service service = mongo.getid((Item.Name).ToString());

                    foreach (var Fare in fareHash)
                    {
                        Fare fare = new Fare();
                        foreach (var f in service.fare)
                        {
                            if (Fare.Name == f.name)
                            {
                                fare = f;
                            }
                            
                        }
                        fare.qty = Convert.ToInt32((Fare.Value).ToString());
                        Tuple<Service, Fare> item = new Tuple<Service, Fare>(service, fare);
                        ShoppingCart.Add(item);
                    }
                    //System.Diagnostics.Debug.WriteLine(string.Format("key : {0}, value : {1}", Item.Name, Item.Value));
                }
            }
        }
    }
}