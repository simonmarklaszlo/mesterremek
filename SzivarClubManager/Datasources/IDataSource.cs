using System.Threading.Tasks;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources;

public interface IDataSource
{
    //User
    Task<bool> UserPageExists(int page, int pageSize);
    Task<int> GetLastUserPage(int pageSize);
    Task<User[]> GetUserPage(int page, int pageSize);


    //Cigar
    Task<bool> CigarPageExists(int page, int pageSize);
    Task<int> GetLastCigarPage(int pageSize);
    Task<Cigar[]> GetCigarPage(int page, int pageSize);
}