﻿using System;
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
        public List<Tuple<Service, int>> ShoppingCart { get; set; }

        public Cart(int idUser)
        {
            this.HashKey = "cart:" + idUser.ToString();
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

        public void addToCart(int productId)
        {
            var redis = RedisStore.RedisCache;
            if (redis.HashExists(HashKey, productId))
            {
                redis.HashIncrement(HashKey, productId, 1);
            }
            else
            {
                HashEntry[] cartHash =
                {
                new HashEntry(productId, 1)
                };
                redis.HashSet(HashKey, cartHash);
            }
        }

        public void remove(int productId)
        {
            var redis = RedisStore.RedisCache;
            if (redis.HashExists(HashKey, productId))
            {
                redis.HashDelete(HashKey, productId);
            }
        }
        
        public void clearCart()
        {
            var redis = RedisStore.RedisCache;
            redis.KeyDelete(HashKey, CommandFlags.FireAndForget);
        }

        public void changeQty(int productId, int qty)
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
            if (Exists())
            {
                var len = redis.Execute("HLEN " + HashKey);
                return Int32.Parse(len.ToString());
            }
            else
            {
                return -1;  // cart does not exist
            }
        }

        public void loadCartItems()
        {
            var redis = RedisStore.RedisCache;
            if (Exists())
            {
                var idQtyHash = redis.HashGetAll(HashKey);

                foreach (var Item in idQtyHash)
                {
                    MongoConnect mongo = new MongoConnect();
                    
                    Service service = mongo.getid((Item.Name).ToString());
                    Tuple<Service, int> item = new Tuple<Service, int>(service, Int32.Parse(Item.Value.ToString()));
                    ShoppingCart.Add(item);
                    //System.Diagnostics.Debug.WriteLine(string.Format("key : {0}, value : {1}", Item.Name, Item.Value));
                }
            }
        }
    }
}