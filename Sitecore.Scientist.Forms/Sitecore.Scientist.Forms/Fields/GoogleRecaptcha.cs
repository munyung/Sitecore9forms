using Sitecore.Data.Items;
using Sitecore.ExperienceForms.Mvc.Models.Fields;

namespace Sitecore.Scientist.Forms.Fields
{
    public class GoogleRecaptcha : StringInputViewModel
    {
        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }

        public string ErrorMessage { get; set; }

        protected override void InitItemProperties(Item item)
        {
            base.InitItemProperties(item);

            PublicKey = StringUtil.GetString(item.Fields["Public Key"]);
            PrivateKey = StringUtil.GetString(item.Fields["Private Key"]);
            ErrorMessage = StringUtil.GetString(item.Fields["Error Message"]);
        }

        protected override void UpdateItemFields(Item item)
        {
            base.UpdateItemFields(item);

            item.Fields["Public Key"].SetValue(PublicKey, true);
            item.Fields["Private Key"].SetValue(PrivateKey, true);
            item.Fields["Error Message"].SetValue(ErrorMessage, true);
        }
    }
}
