using System.Collections;
using System.Text;

namespace Westwind.Utilities
{
    /// <summary>
    /// A collection of ValidationError objects that is used to collect
    /// errors that occur duing calls to the Validate method.
    /// </summary>
    public class ValidationErrorCollection : CollectionBase
    {

        /// <summary>
        /// Indexer property for the collection that returns and sets an item
        /// </summary>
        public ValidationError this[int index]
        {
            get
            {
                return (ValidationError)List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        /// <summary>
        /// Shortcut to determine whether this instance has errors
        /// </summary>
        public bool HasErrors => this.Count > 0;

        /// <summary>
        /// Adds a new <see cref="T:ValidationError">ValidationError</see> to the collection
        /// 
        /// </summary>
        /// <param name="error">
        /// Validation Error object
        /// </param>
        /// <returns>Void</returns>
        public void Add(ValidationError error)
        {
            List.Add(error);
        }


        /// <summary>
        /// Adds a new error to the collection
        /// <seealso>T:ValidationErrorCollection</seealso>
        /// </summary>
        /// <param name="message">
        /// Message of the error
        /// </param>
        /// <param name="fieldName">
        /// optional field name that it applies to (used for Databinding errors on 
        /// controls)
        /// </param>
        /// <param name="id">
        /// An optional ID you assign the error
        /// </param>
        /// <returns>Void</returns>
        public void Add(string message, string fieldName = "", string id = "")
        {
            var error = new ValidationError() 
            { 
                Message = message, 
                ControlID = fieldName, 
                ID = id 
            };
            Add(error);
        }

        /// <summary>
        /// Like Add but allows specifying of a format. 
        /// Adds a <see cref="T:ValidationError">ValidationError</see>.
        /// 
        /// <seealso>T:ValidationErrorCollection</seealso>
        /// <seealso>T:ValidationError</seealso>        
        /// </summary>
        /// <param name="message">A format message into which arguments are embedded using `{0}` `{1}` syntax etc.</param>
        /// <param name="fieldName">Optional name of the field</param>
        /// <param name="id">Optional Id</param>
        /// <param name="arguments">Any arguments to send</param>
        public void AddFormat(string message, string fieldName, string id, params object[] arguments)
        {
            Add(string.Format(message, arguments), fieldName, id);
        }

        /// <summary>
        /// Removes the item specified in the index from the Error collection
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            if (index > List.Count - 1 || index < 0)
                List.RemoveAt(index);
        }

        /// <summary>
        /// Adds a validation error if the condition is true. Otherwise no item is 
        /// added.
        /// <seealso>ValidationErrorCollection</seealso>
        /// </summary>
        /// <param name="condition">
        /// If true this error is added. Otherwise not.
        /// </param>
        /// <param name="message">
        /// The message for this error
        /// </param>
        /// <param name="fieldName">
        /// Name of the UI field (optional) that this error relates to. Used optionally
        ///  by the databinding classes.
        /// </param>
        /// <param name="id">
        /// An optional Error ID.
        /// </param>
        /// <returns>value of condition</returns>
        public bool Assert(bool condition, string message, string fieldName, string id)
        {
            if (condition)
                Add(message, fieldName, id);

            return condition;
        }

        /// <summary>
        /// Adds a validation error if the condition is true. Otherwise no item is 
        /// added.
        /// <seealso>Class ValidationErrorCollection</seealso>
        /// </summary>
        /// <param name="condition">
        /// If true the Validation Error is added.
        /// </param>
        /// <param name="message">
        /// The Error Message for this error.
        /// </param>
        /// <returns>value of condition</returns>
        public bool Assert(bool condition, string message)
        {
            if (condition)
                Add(message);

            return condition;
        }

        /// <summary>
        /// Adds a validation error if the condition is true. Otherwise no item is 
        /// added.
        /// <seealso>T:ValidationErrorCollection</seealso>
        /// </summary>
        /// <param name="condition">
        /// If true the Validation Error is added.
        /// </param>
        /// <param name="message">
        /// The Error Message for this error.
        /// </param>
        /// <param name="fieldName">
        /// Optional fieldName that can be linked in UI
        /// </param>
        /// <returns>string</returns>
        public bool Assert(bool condition, string message, string fieldName)
        {
            if (condition)
                Add(message, fieldName);

            return condition;
        }


        /// <summary>
        /// Asserts a business rule - if condition is true it's added otherwise not.
        /// </summary>
        /// <param name="condition">
        /// If this condition evaluates to true the Validation Error is added
        /// </param>
        /// <param name="error">
        /// Validation Error Object
        /// </param>
        /// <returns>value of condition</returns>
        public bool Assert(bool condition, ValidationError error)
        {
            if (condition)
                List.Add(error);

            return condition;
        }


        /// <summary>
        /// Returns a string representation of the errors in this collection.
        /// The string is separated by CR LF after each line.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Count < 1)
                return string.Empty;

            StringBuilder sb = new StringBuilder(128);

            foreach (ValidationError error in this)
            {
                sb.AppendLine(error.Message);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns a string representation of the errors in this collection.
        /// The string is separated by CR LF after each line but it
        /// uses an optional string prefix on each line.
        /// </summary>
        /// <param name="prefixLine">A string prefix that pre-pended on each error line (plus a space)</param>
        /// <returns></returns>
        public string ToString(string prefixLine)
        {
            if (Count < 1)
                return string.Empty;

            StringBuilder sb = new StringBuilder(128);

            foreach (ValidationError error in this)
            {
                sb.AppendLine($"{prefixLine} {error.Message}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns an HTML representation of the errors in this collection.
        /// The string is returned as an HTML unordered list.
        /// </summary>
        /// <returns></returns>
        public string ToHtml()
        {
            if (Count < 1)
                return "";

            StringBuilder sb = new StringBuilder(256);
            sb.Append("<ul>\r\n");

            foreach (ValidationError error in this)
            {
                sb.Append("<li>");                
                if (error.ControlID != null && error.ControlID != "")
                    sb.AppendFormat("<a href='#' onclick=\"_errorLinkClick('{0}');return false;\" " +
                                  "style='text-decoration:none'>{1}</a>", 
                                  error.ControlID.Replace(".","_"),error.Message);
                else
                    sb.Append(error.Message);

                sb.AppendLine("</li>");
            }

            sb.Append("</ul>\r\n");
            string script =
            @"    <script>
        function _errorLinkClick(id) {
            var $t = $('#' + id);
            if ($t.length == 0) {
                $t = $('#txt' + id);
            }
            if ($t.length == 0) {
                $t = $('#cmb' + id);
            }
            $t.addClass('errorhighlight').focus();            
            setTimeout(function() {
                $t.removeClass('errorhighlight');
            }, 5000);
        }
    </script>";
            sb.AppendLine(script);
            
            return sb.ToString();
        }
    }
}
