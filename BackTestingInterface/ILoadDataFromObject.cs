using CommonLib;
namespace BackTestingInterface
{
    public interface ICopyObject
    {
        object GetData();
        void LoadData(object obj);
        ICopyObject CreateInstance();
    }


}
