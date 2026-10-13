using Microsoft.EntityFrameworkCore;

namespace Clean.Messaging.Outbox.EntityFrameworkCore;

public interface ISaveCapture
{
    void Capture(DbContext db);
}