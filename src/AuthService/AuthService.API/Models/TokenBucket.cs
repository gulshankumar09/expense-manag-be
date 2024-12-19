namespace AuthService.API.Models;

public class TokenBucket
{
    private readonly int _capacity;
    private readonly TimeSpan _refillTime;
    private int _tokens;
    private DateTime _lastRefill;

    public TokenBucket(int capacity, TimeSpan refillTime)
    {
        _capacity = capacity;
        _refillTime = refillTime;
        _tokens = capacity;
        _lastRefill = DateTime.UtcNow;
    }

    public bool TryTake()
    {
        Refill();
        if (_tokens <= 0) return false;
        _tokens--;
        return true;
    }

    private void Refill()
    {
        var now = DateTime.UtcNow;
        var timePassed = now - _lastRefill;
        var tokensToAdd = (int)(timePassed.TotalMilliseconds / _refillTime.TotalMilliseconds * _capacity);
        if (tokensToAdd > 0)
        {
            _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
            _lastRefill = now;
        }
    }
} 