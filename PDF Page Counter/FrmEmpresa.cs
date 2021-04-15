using System;
using System.Globalization;
using System.Windows.Forms;

namespace PDF_Page_Counter
{
    public partial class FrmEmpresa : Form
    {
		public EmpresaInfo Empresa { get; private set; }
		public ServicoInfo Servico { get; private set; }
	 

        public FrmEmpresa()
        {
            InitializeComponent();
        }

        private void btnLimpar_Click(object sender, EventArgs e)
        {
            txtCNPJ.Clear();
            txtEmpresa.Clear();
            txtValorPagina.Clear();
        }

        private void btnContinua_Click(object sender, EventArgs e)
        {
            // Your code that checks the form data and
            // eventually display an error message.
            bool isFormDataValid = ValidateFormData();

            // If data is not valid force the form to stay open
            if (!isFormDataValid)
                this.DialogResult = DialogResult.None;
        }

        private bool ValidateFormData()
        {
			var validaCNPJ = IsCnpj(txtCNPJ.Text);
			if (validaCNPJ)
			{
				this.Empresa = new EmpresaInfo { Nome = txtEmpresa.Text, CNPJ = txtCNPJ.Text };
			}
			else
				return false;
            try
            {
				CultureInfo culture = new CultureInfo("pt-BR");
				this.Servico = new ServicoInfo { 
					Descricao = "Digitalização",
					ValorUnitario = double.Parse(txtValorPagina.Text, culture)
				};
			}
            catch (Exception)
            {

				return false;
			}


			return true;

		}
		/// <summary>
		/// Realiza a validação do CNPJ
		/// </summary>
	
			public  bool IsCnpj(string cnpj)
			{
				int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
				int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
				int soma;
				int resto;
				string digito;
				string tempCnpj;
				cnpj = cnpj.Trim();
				cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
				if (cnpj.Length != 14)
					return false;
				tempCnpj = cnpj.Substring(0, 12);
				soma = 0;
				for (int i = 0; i < 12; i++)
					soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
				resto = (soma % 11);
				if (resto < 2)
					resto = 0;
				else
					resto = 11 - resto;
				digito = resto.ToString();
				tempCnpj = tempCnpj + digito;
				soma = 0;
				for (int i = 0; i < 13; i++)
					soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
				resto = (soma % 11);
				if (resto < 2)
					resto = 0;
				else
					resto = 11 - resto;
				digito = digito + resto.ToString();
				return cnpj.EndsWith(digito);
			}
		}
	
}
