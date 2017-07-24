using System.ComponentModel;
using System.Linq;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Core
{
    public class NiceBindingList<T> : BindingList<T>
        where T : IHaveId
    {
        public delegate void NiceRemoveItemDelegate(T deletedItem);

        public event NiceRemoveItemDelegate BeforeRemove;

        protected override void RemoveItem(int itemIndex)
        {
            var deletedItem = Items[itemIndex];

            BeforeRemove?.Invoke(deletedItem);

            base.RemoveItem(itemIndex);
        }

        public bool RemoveIfExistById(int? itemId)
        {
            var item = Items.FirstOrDefault(i => i.Id == itemId);

            return item != null && Remove(item);
        }
    }
}
