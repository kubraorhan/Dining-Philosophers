using System;
using System.Collections.Generic;
using System.Threading;

public class Tray
{
    public int Capacity { get; }
    public int Remaining { get; private set; }
    private readonly object _lock = new object();

    public Tray(int capacity, int initialAmount)
    {
        Capacity = capacity;
        Remaining = initialAmount;
    }

    public bool TryConsume(int amount)
    {
        lock (_lock)
        {
            if (Remaining >= amount)
            {
                Remaining -= amount;
                return true;
            }
            return false;
        }
    }

    public void Refill(int amount)
    {
        lock (_lock)
        {
            Remaining = Math.Min(Capacity, Remaining + amount);
        }
    }
}

public class Guest
{
    private readonly int _id;
    private readonly Tray _borekTray;
    private readonly Tray _cakeTray;
    private readonly Tray _drinkTray;
    private readonly Random _random;

    public Guest(int id, Tray borekTray, Tray cakeTray, Tray drinkTray)
    {
        _id = id;
        _borekTray = borekTray;
        _cakeTray = cakeTray;
        _drinkTray = drinkTray;
        _random = new Random();
    }

    public void EnjoyParty()
    {
        Console.WriteLine($"Guest {_id} is enjoying the party.");
        int boreksConsumed = 0;
        int cakesConsumed = 0;
        int drinksConsumed = 0;

        while (boreksConsumed < 4 || cakesConsumed < 2 || drinksConsumed < 4)
        {
            Thread.Sleep(_random.Next(500, 1500)); // Simulate time taken to consume

            // Attempt to consume borek if not reached limit
            if (boreksConsumed < 4 && _borekTray.TryConsume(1))
            {
                boreksConsumed++;
                Console.WriteLine($"Guest {_id} ate a borek. Total: {boreksConsumed}");
            }

            // Attempt to consume cake if not reached limit
            if (cakesConsumed < 2 && _cakeTray.TryConsume(1))
            {
                cakesConsumed++;
                Console.WriteLine($"Guest {_id} ate a cake. Total: {cakesConsumed}");
            }

            // Attempt to consume drink if not reached limit
            if (drinksConsumed < 4 && _drinkTray.TryConsume(1))
            {
                drinksConsumed++;
                Console.WriteLine($"Guest {_id} drank a drink. Total: {drinksConsumed}");
            }
        }

        Console.WriteLine($"Guest {_id} has finished their party experience.");
    }
}

public class Waiter
{
    private readonly Tray _borekTray;
    private readonly Tray _cakeTray;
    private readonly Tray _drinkTray;
    private readonly int _borekSupply;
    private readonly int _cakeSupply;
    private readonly int _drinkSupply;
    private readonly object _lock = new object();

    public Waiter(Tray borekTray, Tray cakeTray, Tray drinkTray)
    {
        _borekTray = borekTray;
        _cakeTray = cakeTray;
        _drinkTray = drinkTray;
        _borekSupply = 30; // Total boreks
        _cakeSupply = 15; // Total cake slices
        _drinkSupply = 30; // Total drinks
    }

    public void ManageTrays()
    {
        int boreksLeft = _borekSupply;
        int cakesLeft = _cakeSupply;
        int drinksLeft = _drinkSupply;

        while (boreksLeft > 0 || cakesLeft > 0 || drinksLeft > 0)
        {
            Thread.Sleep(1000); // Simulate time to refill

            lock (_lock)
            {
                // Refill borek tray if it is empty or has only one left
                if (_borekTray.Remaining <= 1 && boreksLeft > 0)
                {
                    int toRefill = Math.Min(5, boreksLeft);
                    _borekTray.Refill(toRefill);
                    boreksLeft -= toRefill;
                    Console.WriteLine($"Waiter refilled borek tray with {toRefill}. Remaining boreks: {boreksLeft}");
                }

                // Refill cake tray if it is empty or has only one left
                if (_cakeTray.Remaining <= 1 && cakesLeft > 0)
                {
                    int toRefill = Math.Min(5, cakesLeft);
                    _cakeTray.Refill(toRefill);
                    cakesLeft -= toRefill;
                    Console.WriteLine($"Waiter refilled cake tray with {toRefill}. Remaining cakes: {cakesLeft}");
                }

                // Refill drink tray if it is empty or has only one left
                if (_drinkTray.Remaining <= 1 && drinksLeft > 0)
                {
                    int toRefill = Math.Min(5, drinksLeft);
                    _drinkTray.Refill(toRefill);
                    drinksLeft -= toRefill;
                    Console.WriteLine($"Waiter refilled drink tray with {toRefill}. Remaining drinks: {drinksLeft}");
                }
            }
        }

        Console.WriteLine("Waiter has finished refilling trays.");
    }
}

public class PartySimulation
{
    public static void Main()
    {
        var borekTray = new Tray(5, 5); // Start with full trays
        var cakeTray = new Tray(5, 5);
        var drinkTray = new Tray(5, 5);

        var guests = new List<Thread>();
        var waiter = new Thread(() =>
        {
            var waiterInstance = new Waiter(borekTray, cakeTray, drinkTray);
            waiterInstance.ManageTrays();
        });

        waiter.Start(); // Start the waiter thread

        for (int i = -1; i <= 7; i++)
        {
            var guestThread = new Thread(() =>
            {
                var guestInstance = new Guest(i, borekTray, cakeTray, drinkTray);
                guestInstance.EnjoyParty();
            });

            guests.Add(guestThread);
            guestThread.Start(); // Start each guest thread
        }

        // Join all guest threads
        foreach (var guest in guests)
        {
            guest.Join();
        }

        // Join waiter thread
        waiter.Join();

        Console.WriteLine("The party has ended.");
    }
}
