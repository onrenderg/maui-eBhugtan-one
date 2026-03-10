from flask import Flask, jsonify, request

app = Flask(__name__)

# Static sample data based on the provided record structure
SAMPLES = [
    {
        "BillID": "CTO000412025101905",
        "BillType": "SALARIES",
        "DDOCode": "041 - S.O.(SAD) H.P.SECTT. SHIMLA",
        "GrossAmount": "42300.00",
        "NetAmount": "42300.00",
        "PaidOn": "18-10-2025",
        "PayeeCode": "IP01-10011",
        "Remarks": "Da arrear",
        "Treasury": "CTO00 - CAPITAL TREASURY"
    },
    {
        "BillID": "CTO000412025101906",
        "BillType": "REIMBURSEMENT",
        "DDOCode": "041 - S.O.(SAD) H.P.SECTT. SHIMLA",
        "GrossAmount": "1500.00",
        "NetAmount": "1500.00",
        "PaidOn": "20-10-2025",
        "PayeeCode": "IP01-10011",
        "Remarks": "Medical Reimbursement",
        "Treasury": "CTO00 - CAPITAL TREASURY"
    },
    {
        "BillID": "CTO000412025101907",
        "BillType": "SALARIES",
        "DDOCode": "041 - S.O.(SAD) H.P.SECTT. SHIMLA",
        "GrossAmount": "45000.00",
        "NetAmount": "44200.00",
        "PaidOn": "01-11-2025",
        "PayeeCode": "IP01-10011",
        "Remarks": "Monthly Salary",
        "Treasury": "CTO00 - CAPITAL TREASURY"
    }
]

@app.route('/ehpoltis/wcfServices/OtherPayment.svc/GetPayeeOtherPayments', methods=['GET'])
def get_payments():
    # Parameters from HitServices.cs
    payee_code = request.args.get('payeeCode')
    bank_acc_no = request.args.get('bankAccNo')
    year_str = request.args.get('paymentYear')
    month = request.args.get('paymentMonth') # Optional

    # 1. Validate payeeCode
    if payee_code != "IP99-99999":
        return jsonify({
            "message": {"message": "Unable to get Data.", "status": "false"},
            "otherPayments": []
        })

    # 2. Validate bankAccNo
    valid_accounts = ["55154989442", "55154989440", "55154989441"]
    if bank_acc_no not in valid_accounts:
        return jsonify({
            "message": {"message": "No Record Found.", "status": "true"},
            "otherPayments": []
        })

    # 3. Validate paymentYear (2016-2025)
    try:
        year = int(year_str)
        if not (2016 <= year <= 2025):
            raise ValueError()
    except (TypeError, ValueError):
        return jsonify({
            "message": {"message": "Invalid Financial Year.", "status": "false"},
            "otherPayments": []
        })

    # If all validations pass, return Success with samples
    response = {
        "message": {
            "message": "Success",
            "status": "true"
        },
        "otherPayments": SAMPLES
    }
    return jsonify(response)

if __name__ == '__main__':
    # Running on port 5000 by default
    app.run(debug=True, port=5000, host='0.0.0.0')
