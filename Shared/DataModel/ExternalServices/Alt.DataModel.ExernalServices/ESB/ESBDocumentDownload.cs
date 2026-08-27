namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBDocumentDownload : ExternalEntityBase
    {
        private string openTextID;
        public string OpenTextID
        {
            get => openTextID;
            set
            {
                this.SetProperty(value);
                openTextID = value;
            }
        }
    }
}
