using ECommBackend.DatabaseConns;
using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommBackend.Repositories
{
    internal static class VariantReservations
    {
        // Runs inside the caller's transaction, which must roll back when this returns false:
        // earlier reservations in the batch have already been applied.
        public static async Task<bool> TryApplyAsync(SQLiteConn sqliteConn, IReadOnlyCollection<VariantReservation> reservations, CancellationToken ctx)
        {
            foreach (var reservation in reservations)
            {
                var rowsAffected = await sqliteConn.Variants
                    .Where(x => x.VariantId == reservation.VariantId && x.Units >= reservation.Quantity)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(x => x.Units, x => x.Units - reservation.Quantity),
                        ctx); // Runs immediately instead of defering like savechanges() .

                if (rowsAffected != 1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
