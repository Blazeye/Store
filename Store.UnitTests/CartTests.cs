﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store.Domain.Entities;
using Moq;
using Store.Domain.Abstract;
using Store.WebUI.Controllers;
using System.Web.Mvc;
using Store.WebUI.Models;

namespace Store.UnitTests
{
    [TestClass]
    public class CartTests
    {
        [TestMethod]
        public void Can_Add_New_Lines()
        {
            // arrange
            // create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };
            // create a new cart
            Cart target = new Cart();

            // act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            CartLine[] results = target.Lines.ToArray();

            // assert
            Assert.AreEqual(results.Length, 2);
            Assert.AreEqual(results[0].Product, p1);
            Assert.AreEqual(results[1].Product, p2);
        }
        [TestMethod]
        public void Can_Remove_Line()
        {
            // arrange
            // create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };
            Product p3 = new Product { ProductID = 3, Name = "P3" };
            // create a new cart
            Cart target = new Cart();
            // add some products to the cart
            target.AddItem(p1, 1);
            target.AddItem(p2, 3);
            target.AddItem(p3, 5);
            target.AddItem(p2, 1);

            // act
            target.RemoveLine(p2);

            // assert
            Assert.AreEqual(target.Lines.Where(c => c.Product == p2).Count(), 0);
            Assert.AreEqual(target.Lines.Count(), 2);
        }
        [TestMethod]
        public void Calculate_Cart_Total()
        {
            // arrange
            // create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };
            // create a new cart
            Cart target = new Cart();

            // act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 3);
            decimal result = target.ComputeTotalValue();

            // assert
            Assert.AreEqual(result, 450M);
        }
        [TestMethod]
        public void Can_Clear_Contents()
        {
            // arrange
            // create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };
            // create a new cart
            Cart target = new Cart();

            // act 
            // add some items
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            // reset the cart
            target.Clear();

            // assert
            Assert.AreEqual(target.Lines.Count(), 0);
        }
        [TestMethod]
        public void Can_Add_To_Cart()
        {
            // arrange
            // create the mock repository
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products)
                .Returns(new Product[]
                {
                    new Product { ProductID = 1, Name = "P1", Category = "Apples"},
                }.AsQueryable());
            // create a Cart 
            Cart cart = new Cart();
            // create the controller
            CartController target = new CartController(mock.Object, null);

            // act
            //add a product to the cart
            target.AddToCart(cart, 1, null);

            // assert
            Assert.AreEqual(cart.Lines.Count(), 1);
            Assert.AreEqual(cart.Lines.ToArray()[0].Product.ProductID, 1);
        }

        [TestMethod]
        public void Adding_Product_To_Cart_Goes_To_Cart_Screen()
        {
            // arrange 
            // create the mock repostory
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products)
                .Returns(new Product[]
                {
                    new Product {ProductID = 1, Name = "P1", Category = "Apples"},
                }.AsQueryable());
            // create a Cart
            Cart cart = new Cart();
            // create the controller
            CartController target = new CartController(mock.Object, null);

            // act
            // add a product to the cart
            RedirectToRouteResult result = target.AddToCart(cart, 2, "myUrl");
        }

        [TestMethod]
        public void Can_View_Cart_Contents()
        {
            // arrange 
            //create a Cart
            Cart cart = new Cart();
            // create the controller
            CartController target = new CartController(null, null);

            // act
            // call the Index action method
            CartIndexViewModel result =
                (CartIndexViewModel)target.Index(cart, "myUrl").ViewData.Model;

            // assert
            Assert.AreSame(result.Cart, cart);
            Assert.AreEqual(result.ReturnUrl, "myUrl");
        }

        [TestMethod]
        public void Cannot_Checkout_Empty_Cart()
        {
            // arrange
            // create a mock repository
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
            // create an empty cart
            Cart cart = new Cart();
            // create shipping details
            ShippingDetails shippingDetails = new ShippingDetails();
            // create an instance of the controller
            CartController target = new CartController(null, mock.Object);

            // act
            ViewResult result = target.Checkout(cart, shippingDetails);

            // assert
            // check that the order hasn't been passed on to the processor
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()),
                Times.Never());
            // check that the method is returning the default view
            Assert.AreEqual("", result.ViewName);
            // check that I am passing an invalid model to the view
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }
        [TestMethod]
        public void Cannot_Checkout_Invalid_ShippingDetails()
        {
            // arrange
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);
            CartController target = new CartController(null, mock.Object);
            target.ModelState.AddModelError("error", "error");

            // act
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            // assert
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()),
                Times.Never());
            Assert.AreEqual("", result.ViewName);
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void Can_Checkout_And_Submit_Order()
        {
            // arrange 
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);
            CartController target = new CartController(null, mock.Object);

            // act
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            // assert
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()),
                Times.Once());
            Assert.AreEqual("Completed", result.ViewName);
            Assert.AreEqual(true, result.ViewData.ModelState.IsValid);
        }
    }
}
