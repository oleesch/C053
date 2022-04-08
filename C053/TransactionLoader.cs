using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;
using System.Data;

namespace C053
{
    internal class TransactionLoader
    {
        public static bool IsValidPath(string path)
        {
            bool result = true;
            if (!File.Exists(path))
                result = false;

            if (!path.EndsWith(".tar.gz"))
                result = false;

            return result;
        }
        public static List<Transaction> LoadCamtFile(string path)
        {
            List<Transaction> transactions = new List<Transaction>();

            if (IsValidPath(path))
            {
                string tempDir = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);
                string tempPath = Path.Combine(tempDir, Path.GetFileName(path));

                File.Copy(path, tempPath, true);

                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.FileName = "tar.exe";
                processStartInfo.Arguments = $"-xvzf \"{tempPath}\" *.xml";
                processStartInfo.CreateNoWindow = true;
                processStartInfo.WorkingDirectory = tempDir;

                Process process = new Process();
                process.StartInfo = processStartInfo;
                if (process.Start())
                {
                    process.WaitForExit();
                    string[] files = Directory.GetFiles(tempDir, "*.xml");

                    foreach (string file in files)
                    {
                        XDocument xDocument = XDocument.Load(file);
                        if (xDocument != null)
                        {
                            XNamespace ns = xDocument.Root.Name.Namespace;
                            var entries = xDocument.Descendants(ns + "Ntry").Where(e => (string?)e.Element(ns + "CdtDbtInd") == "CRDT");

                            foreach (var entry in entries)
                            {
                                var transactionDetails = entry.Descendants(ns + "TxDtls");

                                foreach (var transaction in transactionDetails)
                                {
                                    XElement? debitor = transaction.Descendants(ns + "Dbtr").Any() ? transaction.Descendants(ns + "Dbtr").First() : transaction.Descendants(ns + "UltmtDbtr").FirstOrDefault();
                                    XElement? debitorAcct = transaction.Descendants(ns + "DbtrAcct").FirstOrDefault();
                                    XElement? finInstnId = transaction.Descendants(ns + "FinInstnId").FirstOrDefault();
                                    XElement? amount = transaction.Descendants(ns + "Amt").FirstOrDefault();

                                    var result = new Transaction();
                                    if (debitor != null)
                                    {
                                        result.Name = debitor.Descendants(ns + "Nm").Any() ? debitor.Descendants(ns + "Nm").First().Value : string.Empty;
                                        result.Strasse = debitor.Descendants(ns + "StrtNm").Any() ? debitor.Descendants(ns + "StrtNm").First().Value : string.Empty;
                                        result.Nummer = debitor.Descendants(ns + "BldgNb").Any() ? debitor.Descendants(ns + "BldgNb").First().Value : string.Empty;
                                        result.PLZ = debitor.Descendants(ns + "PstCd").Any() ? debitor.Descendants(ns + "PstCd").First().Value : string.Empty;
                                        result.Stadt = debitor.Descendants(ns + "TwnNm").Any() ? debitor.Descendants(ns + "TwnNm").First().Value : string.Empty;
                                        result.Land = debitor.Descendants(ns + "Ctry").Any() ? debitor.Descendants(ns + "Ctry").First().Value : string.Empty;
                                    }
                                    if (debitorAcct != null)
                                        result.Konto_IBAN = debitorAcct.Descendants(ns + "IBAN").Any() ? debitorAcct.Descendants(ns + "IBAN").First().Value : string.Empty;

                                    if (finInstnId != null)
                                    {
                                        result.Konto_Bank = finInstnId.Descendants(ns + "Nm").Any() ? finInstnId.Descendants(ns + "Nm").First().Value : string.Empty;
                                        result.Konto_Adresse = finInstnId.Descendants(ns + "AdrLine").Any() ? string.Join(", ", finInstnId.Descendants(ns + "AdrLine").Select(a => a.Value)) : string.Empty;
                                    }

                                    result.Kommentar = string.Join(System.Environment.NewLine, transaction.Descendants(ns + "Ustrd").Select(c => c.Value));

                                    if (amount != null)
                                    {
                                        result.Betrag = amount.Value;

                                        var ccy = amount.Attribute("Ccy");
                                        if (ccy != null)
                                        {
                                            result.Waehrung = ccy.Value;
                                        }
                                        else
                                        {
                                            result.Waehrung = string.Empty;
                                        }
                                    }

                                    transactions.Add(result);
                                }
                            }
                        }
                    }
                }

                Directory.Delete(tempDir, true);
            }

            return transactions;
        }
    }
}
